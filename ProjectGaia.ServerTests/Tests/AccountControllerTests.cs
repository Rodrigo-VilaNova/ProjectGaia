using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Validations;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Moq;
using Newtonsoft.Json.Linq;
using NuGet.Protocol;
using ProjectGaia.Server.Controllers;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace ProjectGaia.ServerTests.Tests
{
    public class AccountControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AppDbContext _context;
        private readonly ConfirmationService _confirmationService;
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;
        private readonly AccountController _controller;

        public AccountControllerTests(ITestOutputHelper testOutputHelper)
        {
            Environment.SetEnvironmentVariable("IS_UNIT_TEST", "-");

            _testOutputHelper = testOutputHelper;

            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBAccountTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _confirmationService = new ConfirmationService();
            _passwordService = new PasswordService();
            _tokenService = new TokenService();
            _controller = new AccountController(_context, _confirmationService, _passwordService, _tokenService);
        }

        private ObjectResult AssertStatusCode(int statusCode, object? response)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(statusCode, assertResponse.StatusCode);
            return assertResponse;
        }

        [Fact]
        public async Task RegisterAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var accountDTO = new AccountDTO { Password = "ValidPass1!" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task RegisterAccount_InvalidPassword_ReturnsBadRequest()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "short" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task RegisterAccount_EmailAlreadyExists_ReturnsConflict()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "testuser@example.com", Password = "ValidPass1!" };

            await _controller.RegisterAccount(accountDTO);
            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(409, result);
        }

        [Fact]
        public async Task RegisterAccount_ValidInput_ReturnsAccepted()
        {
            var accountDTO = new AccountDTO { Email = "test@example.com", Password = "ValidPass1!", Name = "Test User" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(202, result);
        }

        [Fact]
        public async Task ConfirmAccount_ValidInput_ReturnsCreated()
        {
            string token = "UserFiveToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(201, result);
        }

        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsGone()
        {
            string token = "UserSixToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(410, result);
        }

        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsNotFound()
        {
            string token = "UserFiveToken-Invalid";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(404, result);
        }

        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsBadRequest()
        {
            var result = await _controller.ConfirmAccount(null);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task SendRecoveryEmail_ValidInput_ReturnsAccepted()
        {
            RecoveryDTO recoveryDTO = new RecoveryDTO
            {
                Email = "User2@gmail.com"
            };

            var result = await _controller.SendRecoveryEmail(recoveryDTO);

            AssertStatusCode(202, result);
        }

        [Fact]
        public async Task SendRecoveryEmail_InvalidInput_ReturnsNotFound()
        {
            RecoveryDTO recoveryDTO = new RecoveryDTO
            {
                Email = "User0-Invalid@gmail.com"
            };

            var result = await _controller.SendRecoveryEmail(recoveryDTO);

            AssertStatusCode(404, result);
        }

        [Fact]
        public async Task SendRecoveryEmail_BadInput_ReturnsBadRequest()
        {
            RecoveryDTO recoveryDTO = new RecoveryDTO
            {
                Email = null
            };

            var result = await _controller.SendRecoveryEmail(recoveryDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task SendRecoveryEmail_DuplicateRequest_ReturnsConflict()
        {
            RecoveryDTO recoveryDTO = new RecoveryDTO
            {
                Email = "User0@gmail.com"
            };

            var result = await _controller.SendRecoveryEmail(recoveryDTO);

            AssertStatusCode(409, result);
        }

        [Fact]
        public async Task ResetPassword_ValidInput_ReturnsOk()
        {
            string token = "UserZeroToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            ResetDTO resetDTO = new ResetDTO
            {
                Token = hexToken,
                Password = "User01@gmail.com"
            };

            var result = await _controller.ResetPassword(resetDTO);

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task ResetPassword_InvalidToken_ReturnsNotFound()
        {
            string token = "UserZeroToken-Invalid";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            ResetDTO resetDTO = new ResetDTO
            {
                Token = hexToken,
                Password = "User01@gmail.com"
            };

            var result = await _controller.ResetPassword(resetDTO);

            AssertStatusCode(404, result);
        }

        [Fact]
        public async Task ResetPassword_ExpiredToken_ReturnsGone()
        {
            string token = "UserOneToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            ResetDTO resetDTO = new ResetDTO
            {
                Token = hexToken,
                Password = "User11@gmail.com"
            };

            var result = await _controller.ResetPassword(resetDTO);

            AssertStatusCode(410, result);
        }

        [Fact]
        public async Task ResetPassword_InvalidPassword_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            ResetDTO resetDTO = new ResetDTO
            {
                Token = hexToken,
                Password = "123"
            };

            var result = await _controller.ResetPassword(resetDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task LoginAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var loginDTO = new LoginDTO { Password = "ValidPass1!" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task LoginAccount_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDTO = new LoginDTO { Email = "test@example.com", Password = "WrongPassword" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task LoginAccount_AccountBlocked_ReturnsForbidden()
        {
            var loginDTO = new LoginDTO { Email = "User0@gmail.com", Password = "User0@gmail.com" };

            Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == 3);

            Assert.NotNull(account);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task LoginAccount_ValidCredentials_ReturnsOk()
        {
            var loginDTO = new LoginDTO { Email = "User0@gmail.com", Password = "User0@gmail.com" };

            var result = await _controller.LoginAccount(loginDTO);

            var assertResult = AssertStatusCode(200, result);
            Assert.NotNull(assertResult.Value);
            Assert.Contains("Token", assertResult.Value.ToString());
        }

        [Fact]
        public async Task LogoutAccount_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.LogoutAccount();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task LogoutAccount_ValidToken_ReturnsOk()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.LogoutAccount();

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task DeleteAccount_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task DeleteAccount_AccountBlocked_ReturnsForbidden()
        {
            Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == 3);

            Assert.NotNull(account);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task DeleteAccount_ValidToken_ReturnsOk()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task ChangePassword_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "2323", NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task ChangePassword_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "User0@gmail.com", NewPassword = "User01@gmail.com" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task ChangePassword_AccountBlocked_ReturnsForbidden()
        {
            Account? account = await _context.Accounts.FirstOrDefaultAsync(a => a.ID == 3);

            Assert.NotNull(account);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "User0@gmail.com", NewPassword = "User01@gmail.com" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_ReturnsUnauthorized()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "Wr0ngP455w0rd!", NewPassword = "User01@gmail.com" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task ChangePassword_InvalidNewPassword_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "User0@gmail.com", NewPassword = "user0" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task ChangePassword_ValidInput_ReturnsOk()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "User0@gmail.com", NewPassword = "User01@gmail.com" };

            var result = await _controller.ChangePassword(passwordDTO);

            var test = AssertStatusCode(200, result);

            var loginDTO = new LoginDTO { Email = "User0@gmail.com", Password = passwordDTO.NewPassword };

            var resultLogin = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(200, result);
        }
    }
}
