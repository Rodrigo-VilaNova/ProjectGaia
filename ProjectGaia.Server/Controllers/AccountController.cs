using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    [ApiController]
    [Route("account")]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;

        public AccountController(AppDbContext context, PasswordService passwordService, TokenService tokenService)
        {
            _context = context;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        // POST: Register account
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromBody] AccountDTO accountDTO)
        {
            if (!ModelState.IsValid || accountDTO.Password == null)
            {
                return BadRequest(ModelState); // Return a 400 BadRequest if validation fails
            }

            if (!_passwordService.IsValidPassword(accountDTO.Password))
            {
                return BadRequest("Password must be between 8 and 128 characters, and include at least one uppercase letter, one lowercase letter, one number, and one special character."); // Return a 400 BadRequest if the password does not comply with the requirements
            }

            bool alreadyExists = await _context.Accounts.AnyAsync(a => a.Email == accountDTO.Email);
            if (alreadyExists)
            {
                return Conflict("An account with this email already exists."); // Return a 409 Conflict if the email is taken
            }

            var account = new Account
            {
                Name = accountDTO.Name,
                Email = accountDTO.Email,
                Password = _passwordService.HashPassword(accountDTO.Password),
                Type = AccountType.User,
                Status = AccountStatus.Active
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var accountEntry = _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
                int accountID = accountEntry.Entity.ID;

                string textToken = await _tokenService.GenerateSessionToken(_context, accountID);

                AccessLog accessLog = new AccessLog
                {
                    Date = DateTime.UtcNow,
                    AccountID = accountID
                };

                _context.AccessLogs.Add(accessLog);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Created("", $"{{ \"Token\": \"{textToken}\" }}");
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Error creating account/session");
            }
            
        }

        // POST: Login into account
        [HttpPost("login")]
        public async Task<IActionResult> LoginAccount([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid || loginDTO.Password == null) return BadRequest(ModelState); // Return a 400 BadRequest if validation fails

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
                }

                return Unauthorized("Invalid email or password.");
            } 
#pragma warning restore CS8604 // Possible null reference argument.

            if (account.Status == AccountStatus.Blocked) return StatusCodeResult((403, "Account is blocked and login is not allowed."));

            int accountID = account.ID;

            string textToken = await _tokenService.GenerateSessionToken(_context, accountID);

            AccessLog accessLog = new AccessLog
            {
                Date = DateTime.UtcNow,
                AccountID = accountID
            };

            _context.AccessLogs.Add(accessLog);
            await _context.SaveChangesAsync();

            return Ok($"{{ \"Token\": \"{textToken}\" }}");
        }

        // DELETE: Logout from account
        [HttpDelete("logout")]
        public async Task<IActionResult> LogoutAccount()
        {
            var result = _tokenService.GetToken(Request);
            if (result.token == null) return StatusCodeResult(result.status);

            Session? session = await _tokenService.GetSession(_context, result.token);
            if (session == null) return Unauthorized("Invalid session token");

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return Ok("Session closed successfully.");
        }

        // DELETE: Delete account
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAccount()
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);
            if (account.Status == AccountStatus.Blocked) return StatusCodeResult((403, "Account is blocked and cannot be deleted."));

            await _context.Sessions.Where(s => s.AccountID == account.ID).ExecuteDeleteAsync();
            await _context.AccessLogs.Where(al => al.AccountID == account.ID).ExecuteDeleteAsync();
            await _context.ErrorLogs.Where(el => el.AccountID == account.ID).ExecuteDeleteAsync();
            await _context.Accounts.Where(a => a.ID == account.ID).ExecuteDeleteAsync();

            return Ok("Account and related data deleted successfully.");
        }

        // PUT: Change password
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] PasswordDTO passwordDTO)
        {
            if (!ModelState.IsValid || passwordDTO.OldPassword == null || passwordDTO.NewPassword == null) return BadRequest(ModelState);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _tokenService.GetAccount(_context, Request, true);
                Account? account = result.account;

                if (account == null) return StatusCodeResult(result.status);
                if (account.Status == AccountStatus.Blocked) return StatusCodeResult((403, "Account is blocked and can't change password."));

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

                    return Unauthorized("Old password and account password do not match.");
                }

                if (!_passwordService.IsValidPassword(passwordDTO.NewPassword))
                {
                    return BadRequest("Password must be between 8 and 128 characters, and include at least one uppercase letter, one lowercase letter, one number, and one special character."); // Return a 400 BadRequest if the password does not comply with the requirements
                }

                _context.ChangeTracker.Clear();
                account.Password = _passwordService.HashPassword(passwordDTO.NewPassword); 
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("Password updated successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await transaction.RollbackAsync();
                return StatusCode(500, "Error updating password");
            }
        }

        private ObjectResult StatusCodeResult((int code, string? message)? status)
        {
            return StatusCode(status?.code ?? 0, status?.message);
        } 
    }
}
