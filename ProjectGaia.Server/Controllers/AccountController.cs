using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Identity.Client;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ConfirmationService _confirmationService;
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;

        public AccountController(AppDbContext context, ConfirmationService confirmationService, PasswordService passwordService, TokenService tokenService)
        {
            _context = context;
            _confirmationService = confirmationService;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        // POST: Register account
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
                    var parameters = new { token = hexToken};
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

        // GET: Confirm account
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

        // POST: Login into account
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

        // POST: Send password reset email
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

                    string fullUrl = $"{scheme}://{host}{path}";

                    string subject = $"Hello there, {account.Name}, a password reset was requested for your Project Gaia account";
                    string body = $"To reset your Project Gaia account password please open the following link:\n{fullUrl}";

                    Console.WriteLine($"Activation URL: {fullUrl}");

                    EmailSender emailSender = new EmailSender();
                    //await emailSender.SendEmailAsync(recoveryDTO.Email, subject, body);
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

        // Put: Reset account password
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

        // DELETE: Logout from account
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

        // DELETE: Delete account
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

        // PUT: Change password
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

        private ObjectResult StatusCodeResult((int code, string? message)? status)
        {
            return StatusCode(status?.code ?? 0, status?.message);
        } 
    }
}
