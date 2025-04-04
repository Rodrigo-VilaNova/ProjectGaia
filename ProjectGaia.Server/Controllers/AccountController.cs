using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        /// <summary>
        /// Contexto da base de dados utilizado para interagir com o banco de dados da aplicação.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Serviço responsável por gerar e validar tokens de confirmação de conta.
        /// </summary>
        private readonly ConfirmationService _confirmationService;

        /// <summary>
        /// Serviço utilizado para validação e hash de palavras-passe, garantindo segurança no armazenamento.
        /// </summary>
        private readonly PasswordService _passwordService;

        /// <summary>
        /// Serviço de autenticação e verificação de token responsável por validar e obter informações sobre a conta do utilizador a partir do token do request.
        /// </summary>
        private readonly TokenService _tokenService;

        /// <summary>
        /// Inicializa uma nova instância de <see cref="AccountController"/> com os serviços necessários injetados.
        /// </summary>
        /// <param name="context">Contexto da base de dados para operações com a base de dados.</param>
        /// <param name="confirmationService">Serviço para geração e validação de tokens de confirmação.</param>
        /// <param name="passwordService">Serviço de hash e verificação de senhas.</param>
        /// <param name="tokenService">Serviço de autenticação e extração de conta com base em token.</param>
        public AccountController(AppDbContext context, ConfirmationService confirmationService, PasswordService passwordService, TokenService tokenService)
        {
            _context = context;
            _confirmationService = confirmationService;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Inicia o registo de uma nova conta e envia um email de confirmação.
        /// </summary>
        /// <param name="accountDTO">Objeto contendo o nome, e-mail e password do utilizador a registar.</param>
        /// <returns>Retorna 202 se o email de confirmação foi enviado com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountDTO accountDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid || accountDTO.Name == null || accountDTO.Email == null || accountDTO.Password == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, ModelState);
                }

                if (string.IsNullOrWhiteSpace(accountDTO.Name) || accountDTO.Name.Trim().Length > 64)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, "Name length must be between 1 and 64");
                }

                if (!_passwordService.IsValidPassword(accountDTO.Password))
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, "Password must be between 8 and 128 characters, and include at least one uppercase letter, one lowercase letter, one number, and one special character");
                }

                bool confirmationAlreadyExists = await _context.Confirmations.AnyAsync(c => c.Email == accountDTO.Email);
                if (confirmationAlreadyExists)
                {
                    Confirmation currentConfirmation = await _context.Confirmations.FirstAsync(c => c.Email == accountDTO.Email);

                    if (currentConfirmation.Expiration > DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(409, "An account confirmation with this email is already pending");
                    }
                    else
                    {
                        _context.Confirmations.Remove(currentConfirmation);
                        await _context.SaveChangesAsync();
                    }
                }

                bool accountAlreadyExists = await _context.Accounts.AnyAsync(a => a.Email == accountDTO.Email);
                if (accountAlreadyExists)
                {
                    await transaction.CommitAsync();
                    return StatusCode(409, "An account with this email already exists");
                }

                byte[] token;
                byte[] hashedToken;
                string hexToken;
                string hexHashedToken;

                while (true)
                {
                    token = _confirmationService.GenerateRandomToken();
                    hashedToken = _confirmationService.HashToken(token);
                    hexHashedToken = Convert.ToHexString(hashedToken);
                    bool isUnique = !await _context.Confirmations.AnyAsync(t => t.Token == hexHashedToken);
                    if (isUnique) break;
                }
                hexToken = Convert.ToHexString(token);

                Confirmation confirmation = new Confirmation
                {
                    Token = hexHashedToken,
                    Expiration = DateTime.UtcNow.AddMinutes(30),
                    Name = accountDTO.Name.Trim(),
                    Email = accountDTO.Email,
                    Password = _passwordService.HashPassword(accountDTO.Password),
                };

                bool isUnitTest = Environment.GetEnvironmentVariable("IS_UNIT_TEST") != null;
                if (!isUnitTest)
                {
                    var scheme = Request.Scheme;
                    var host = Request.Host;
                    var parameters = new { token = hexToken };
                    var path = Url.Action("ConfirmAccount", "Account", parameters);

                    string fullUrl = $"{scheme}://{host}{path}".Replace("/api/account/", "/");

                    string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (environment == "Development") fullUrl = fullUrl.Replace("https://localhost:5001/", "http://localhost:5002/");

                    string subject = $"Welcome to Project Gaia, {confirmation.Name}!";
                    string body = $"To complete the registration of your Project Gaia account please open the following link:\n{fullUrl}";

                    Console.WriteLine($"Activation URL: {fullUrl}");

                    EmailSender emailSender = new EmailSender();
                    await emailSender.SendEmailAsync(confirmation.Email, subject, body);
                }

                await _context.Confirmations.AddAsync(confirmation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(202, "A confirmation email was sent if the email exists");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error creating account");
            }
        }

        /// <summary>
        /// Confirma uma conta através do token enviado por email.
        /// </summary>
        /// <param name="token">Token de confirmação da conta.</param>
        /// <returns>Retorna 201 se a conta foi criada com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpGet("confirm")]
        public async Task<IActionResult> ConfirmAccount([FromQuery] string? token)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (token == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, "Token parameter missing");
                }

                string hexHashedToken = Convert.ToHexString(_confirmationService.HashToken(token));
                Confirmation? confirmation = await _context.Confirmations.FirstOrDefaultAsync(c => c.Token == hexHashedToken);

                if (confirmation == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(404, "Invalid token");
                }

                if (confirmation.Expiration <= DateTime.UtcNow)
                {
                    _context.Confirmations.Remove(confirmation);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return StatusCode(410, "Token expired");
                }

                Account account = new Account
                {
                    Name = confirmation.Name,
                    Email = confirmation.Email,
                    Password = confirmation.Password,
                    Type = AccountType.User,
                    Status = AccountStatus.Active
                };

                await _context.Accounts.AddAsync(account);
                _context.Confirmations.Remove(confirmation);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return StatusCode(201, "Account confirmation successful");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error confirming account");
            }
        }

        /// <summary>
        /// Inicia sessão na conta do utilizador através de email e palavra-passe.
        /// </summary>
        /// <param name="loginDTO">DTO com as credenciais de início de sessão.</param>
        /// <returns>Retorna 200 e um token de sessão se for bem-sucedido, ou um código de erro apropriado caso contrário.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> LoginAccount([FromBody] LoginDTO loginDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid || loginDTO.Password == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, ModelState);
                }

                Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == loginDTO.Email);
#pragma warning disable CS8604 // Possible null reference argument.
                if (account == null || !_passwordService.IsCorrectPassword(loginDTO.Password, account.Password)) // Return a 401 Unauthorized if the email/password combination is incorrect
                {
                    if (account != null)
                    {
                        ErrorLog errorLog = new ErrorLog
                        {
                            Date = DateTime.UtcNow,
                            Type = "Login attempt, incorrect password",
                            AccountID = account.ID,
                        };

                        _context.ErrorLogs.Add(errorLog);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    else await transaction.RollbackAsync();

                    return StatusCode(401, "Invalid email or password");
                }
#pragma warning restore CS8604 // Possible null reference argument.

                if (account.Status == AccountStatus.Blocked)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(403, "Account is blocked and login is not allowed");
                }

                int accountID = account.ID;

                string textToken = await _tokenService.GenerateSessionToken(_context, accountID);

                AccessLog accessLog = new AccessLog
                {
                    Date = DateTime.UtcNow,
                    AccountID = accountID
                };

                _context.AccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, $"{{ \"Token\": \"{textToken}\" }}");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error logging into account");
            }
        }

        /// <summary>
        /// Envia um email de recuperação de palavra-passe para o endereço de email registado.
        /// </summary>
        /// <param name="recoveryDTO">DTO com o endereço de email do utilizador.</param>
        /// <returns>Retorna 202 se o email for enviado com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpPost("recovery")]
        public async Task<IActionResult> SendRecoveryEmail([FromBody] RecoveryDTO recoveryDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid || recoveryDTO.Email == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, ModelState);
                }

                Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.Email == recoveryDTO.Email);
                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(404, "Account doesn't exist");
                }

                bool recoveryAlreadyExists = await _context.Recoveries.AnyAsync(r => r.AccountID == account.ID);
                if (recoveryAlreadyExists)
                {
                    Recovery currentRecovery = await _context.Recoveries.FirstAsync(r => r.AccountID == account.ID);

                    if (currentRecovery.Expiration > DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return StatusCode(409, "A password reset for this account is already pending, try again later");
                    }
                    else
                    {
                        _context.Recoveries.Remove(currentRecovery);
                        await _context.SaveChangesAsync();
                    }
                }

                byte[] token;
                byte[] hashedToken;
                string hexToken;
                string hexHashedToken;

                while (true)
                {
                    token = _confirmationService.GenerateRandomToken();
                    hashedToken = _confirmationService.HashToken(token);
                    hexHashedToken = Convert.ToHexString(hashedToken);
                    bool isUnique = !await _context.Recoveries.AnyAsync(r => r.Token == hexHashedToken);
                    if (isUnique) break;
                }
                hexToken = Convert.ToHexString(token);

                Recovery recovery = new Recovery
                {
                    Token = hexHashedToken,
                    Expiration = DateTime.UtcNow.AddMinutes(30),
                    AccountID = account.ID
                };

                bool isUnitTest = Environment.GetEnvironmentVariable("IS_UNIT_TEST") != null;
                if (!isUnitTest)
                {
                    var scheme = Request.Scheme;
                    var host = Request.Host;
                    var parameters = new { token = hexToken };
                    var path = Url.Action("ResetPassword", "Account", parameters);

                    string fullUrl = $"{scheme}://{host}{path}".Replace("/api/account/", "/");

                    string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (environment == "Development") fullUrl = fullUrl.Replace("https://localhost:5001/", "http://localhost:5002/");

                    string subject = $"Hello there, {account.Name}, a password reset was requested for your Project Gaia account";
                    string body = $"To reset your Project Gaia account password please open the following link:\n{fullUrl}";

                    Console.WriteLine($"Activation URL: {fullUrl}");

                    EmailSender emailSender = new EmailSender();
                    await emailSender.SendEmailAsync(recoveryDTO.Email, subject, body);
                }

                await _context.Recoveries.AddAsync(recovery);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(202, "A password reset link was sent to your email");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error creating account/session");
            }
        }

        /// <summary>
        /// Redefine a palavra-passe do utilizador usando um token de recuperação válido.
        /// </summary>
        /// <param name="resetDTO">DTO com o token de recuperação e a nova palavra-passe.</param>
        /// <returns>Retorna 200 se a palavra-passe for redefinida com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpPut("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetDTO resetDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (!ModelState.IsValid || resetDTO.Token == null || resetDTO.Password == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, ModelState);
                }

                string hexHashedToken = Convert.ToHexString(_confirmationService.HashToken(resetDTO.Token));
                Recovery? recovery = await _context.Recoveries.FirstOrDefaultAsync(r => r.Token == hexHashedToken);

                if (recovery == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(404, "Invalid token");
                }

                if (recovery.Expiration <= DateTime.UtcNow)
                {
                    _context.Recoveries.Remove(recovery);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return StatusCode(410, "Token expired");
                }

                if (!_passwordService.IsValidPassword(resetDTO.Password))
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, "Password must be between 8 and 128 characters, and include at least one uppercase letter, one lowercase letter, one number, and one special character");
                }

                Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == recovery.AccountID);
                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(404, "Account doesn't exist");
                }

                account.Password = _passwordService.HashPassword(resetDTO.Password);
                _context.Accounts.Update(account);
                _context.Recoveries.Remove(recovery);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return StatusCode(200, "Password reset successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error confirming account");
            }
        }

        /// <summary>
        /// Termina a sessão atual do utilizador, removendo o token ativo.
        /// </summary>
        /// <returns>Retorna 200 se a sessão for terminada com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpDelete("logout")]
        public async Task<IActionResult> LogoutAccount()
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = _tokenService.GetToken(Request);
                if (result.token == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCodeResult(result.status);
                }

                Session? session = await _tokenService.GetSession(_context, result.token);
                if (session == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(401, "Invalid session token");
                }

                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Session closed successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error logging out of account");
            }
        }

        /// <summary>
        /// Elimina permanentemente a conta do utilizador e todos os dados associados.
        /// </summary>
        /// <returns>Retorna 200 se a conta for eliminada com sucesso, ou um código de erro apropriado caso contrário.</returns>
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _tokenService.GetAccount(_context, Request);
                Account? account = result.account;

                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCodeResult(result.status);
                }

                if (account.Status == AccountStatus.Blocked)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(403, "Account is blocked and cannot be deleted");
                }

                await _context.Events.Where(e => e.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.Invoices.Where(i => i.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.Recoveries.Where(r => r.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.Sessions.Where(s => s.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.AccessLogs.Where(al => al.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.ErrorLogs.Where(el => el.AccountID == account.ID).ExecuteDeleteAsync();
                await _context.Accounts.Where(a => a.ID == account.ID).ExecuteDeleteAsync();

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Account and related data deleted successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error deleting account");
            }
        }

        /// <summary>
        /// Altera a palavra-passe da conta após validar a palavra-passe atual.
        /// </summary>
        /// <param name="passwordDTO">DTO com a palavra-passe atual e a nova palavra-passe.</param>
        /// <returns>Retorna 200 se a alteração for bem-sucedida, ou um código de erro apropriado caso contrário.</returns>
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO passwordDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {

                if (!ModelState.IsValid || passwordDTO.OldPassword == null || passwordDTO.NewPassword == null)
                {
                    await transaction.RollbackAsync();
                    return BadRequest(ModelState);
                }

                var result = await _tokenService.GetAccount(_context, Request);
                Account? account = result.account;

                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return StatusCodeResult(result.status);
                }

                if (account.Status == AccountStatus.Blocked)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(403, "Account is blocked and can't change password");
                }

                if (!_passwordService.IsCorrectPassword(passwordDTO.OldPassword, account.Password ?? []))
                {
                    ErrorLog errorLog = new ErrorLog
                    {
                        Date = DateTime.UtcNow,
                        Type = "Password change attempt, incorrect old password",
                        AccountID = account.ID,
                    };

                    _context.ErrorLogs.Add(errorLog);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return StatusCode(401, "Old password and account password do not match");
                }

                if (!_passwordService.IsValidPassword(passwordDTO.NewPassword))
                {
                    await transaction.RollbackAsync();
                    return StatusCode(400, "Password must be between 8 and 128 characters, and include at least one uppercase letter, one lowercase letter, one number, and one special character"); // Return a 400 BadRequest if the password does not comply with the requirements
                }

                _context.ChangeTracker.Clear();
                account.Password = _passwordService.HashPassword(passwordDTO.NewPassword);
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return StatusCode(200, "Password updated successfully");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error updating password");
            }
        }

        /// <summary>
        /// Retorna o código de status da resposta, com base no tuple de status fornecido.
        /// </summary>
        private ObjectResult StatusCodeResult((int code, string? message)? status)
        {
            return StatusCode(status?.code ?? 0, status?.message);
        }
    }
}
