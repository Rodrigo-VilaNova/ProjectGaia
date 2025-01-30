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
        private readonly PasswordService _passwordService;
        private readonly TokenService _tokenService;
        private readonly AccountController _controller;

        public AccountControllerTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;

            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBAccountTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();


            _passwordService = new PasswordService();
            _tokenService = new TokenService();
            _controller = new AccountController(_context, _passwordService, _tokenService);
        }

        private ObjectResult AssertStatusCode(object? response, int statusCode)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(assertResponse.StatusCode, statusCode);
            return assertResponse;
        }

        [Fact]
        public async Task RegisterAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var accountDTO = new AccountDTO { Password = "ValidPass1!" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(result, 400);

            
        }

        [Fact]
        public async Task RegisterAccount_InvalidPassword_ReturnsBadRequest()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "short" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(result, 400);
        }

        [Fact]
        public async Task RegisterAccount_EmailAlreadyExists_ReturnsConflict()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "testuser@example.com", Password = "ValidPass1!" };

            await _controller.RegisterAccount(accountDTO);
            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(result, 409);
        }

        [Fact]
        public async Task RegisterAccount_ValidInput_ReturnsCreated()
        {
            var accountDTO = new AccountDTO { Email = "test@example.com", Password = "ValidPass1!", Name = "Test User" };

            var result = await _controller.RegisterAccount(accountDTO);

            ObjectResult assertResult = AssertStatusCode(result, 201);
            Assert.NotNull(assertResult.Value);
            Assert.Contains("Token", assertResult.Value.ToString());
        }

        [Fact]
        public async Task LoginAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var loginDTO = new LoginDTO { Password = "ValidPass1!" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(result, 400);
        }

        [Fact]
        public async Task LoginAccount_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDTO = new LoginDTO { Email = "test@example.com", Password = "WrongPassword" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task LoginAccount_AccountBlocked_ReturnsForbidden()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var loginDTO = new LoginDTO { Email = accountDTO.Email, Password = accountDTO.Password };

            await _controller.RegisterAccount(accountDTO);
            Account? account = await _context.Accounts.OrderBy(a => a.ID).LastOrDefaultAsync();

            Assert.NotNull(account);
            Assert.Equal(accountDTO.Email, loginDTO.Email);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(result, 403);
        }

        [Fact]
        public async Task LoginAccount_ValidCredentials_ReturnsOk()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var loginDTO = new LoginDTO { Email = accountDTO.Email, Password = accountDTO.Password };

            await _controller.RegisterAccount(accountDTO);

            var result = await _controller.LoginAccount(loginDTO);

            var assertResult = AssertStatusCode(result, 200);
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

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task LogoutAccount_ValidToken_ReturnsOk()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.LogoutAccount();

            AssertStatusCode(result, 200);
        }

        [Fact]
        public async Task DeleteAccount_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task DeleteAccount_AccountBlocked_ReturnsForbidden()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);
            Account? account = await _context.Accounts.OrderBy(a => a.ID).LastOrDefaultAsync();

            Assert.NotNull(account);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(result, 403);
        }

        [Fact]
        public async Task DeleteAccount_ValidToken_ReturnsOk()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(result, 200);
        }

        [Fact]
        public async Task ChangePassword_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "2323", NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task ChangePassword_InvalidToken_ReturnsUnauthorized()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = accountDTO.Password, NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task ChangePassword_AccountBlocked_ReturnsForbidden()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);
            Account? account = await _context.Accounts.OrderBy(a => a.ID).LastOrDefaultAsync();

            Assert.NotNull(account);

            account.Status = AccountStatus.Blocked;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = accountDTO.Password, NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(result, 403);
        }

        [Fact]
        public async Task ChangePassword_InvalidOldPassword_ReturnsUnauthorized()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "sus amogus", NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task ChangePassword_InvalidNewPassword_ReturnsBadRequest()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = accountDTO.Password, NewPassword = "amogus" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(result, 400);
        }

        [Fact]
        public async Task ChangePassword_ValidInput_ReturnsOk()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "ValidPass1!" };
            var resultRegister = await _controller.RegisterAccount(accountDTO);

            var createdResult = Assert.IsType<CreatedResult>(resultRegister);
            dynamic response = JObject.Parse(createdResult.Value?.ToString() ?? "");

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {response.Token}";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = accountDTO.Password, NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            var test = Assert.IsType<OkObjectResult>(result);

            var loginDTO = new LoginDTO { Email = accountDTO.Email, Password = passwordDTO.NewPassword };

            var resultLogin = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(result, 200);
        }
    }
}
