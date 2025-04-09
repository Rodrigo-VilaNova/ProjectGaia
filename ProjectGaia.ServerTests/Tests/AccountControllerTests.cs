using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Controllers;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;
using Xunit.Abstractions;

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

            bool isGitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

            string connectionString = isGitHubActions ?
                $"Server=localhost,1433;Database=ProjectGaiaDBAccountTests;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False;" :
                "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBAccountTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new TestDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _confirmationService = new ConfirmationService();
            _passwordService = new PasswordService();
            _tokenService = new TokenService();
            _controller = new AccountController(_context, _confirmationService, _passwordService, _tokenService);
        }

        /// <summary>
        /// Verifica se a resposta recebida é do tipo <see cref="ObjectResult"/> e se o código de status retornado é o esperado.
        /// </summary>
        /// <param name="statusCode">Código de status HTTP esperado (por exemplo, 200, 400, 404).</param>
        /// <param name="response">Objeto de resposta retornado pelo método do controller.</param>
        /// <returns>Instância de <see cref="ObjectResult"/> contendo a resposta verificada.</returns>
        private ObjectResult AssertStatusCode(int statusCode, object? response)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(statusCode, assertResponse.StatusCode);
            return assertResponse;
        }

        /// <summary>
        /// Deve retornar BadRequest (400) quando o modelo estiver inválido (ex: email ausente).
        /// </summary>
        [Fact]
        public async Task RegisterAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var accountDTO = new AccountDTO { Password = "ValidPass1!" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(400, result);
        }

        /// <summary>
        /// Deve retornar BadRequest (400) quando a senha não atender aos critérios de validação.
        /// </summary>
        [Fact]
        public async Task RegisterAccount_InvalidPassword_ReturnsBadRequest()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "test@example.com", Password = "short" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(400, result);
        }

        /// <summary>
        /// Deve retornar Conflict (409) ao tentar registrar um e-mail já existente.
        /// </summary>
        [Fact]
        public async Task RegisterAccount_EmailAlreadyExists_ReturnsConflict()
        {
            var accountDTO = new AccountDTO { Name = "Test User", Email = "testuser@example.com", Password = "ValidPass1!" };

            await _controller.RegisterAccount(accountDTO);
            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(409, result);
        }

        /// <summary>
        /// Deve retornar Accepted (202) quando os dados estiverem válidos.
        /// </summary>
        [Fact]
        public async Task RegisterAccount_ValidInput_ReturnsAccepted()
        {
            var accountDTO = new AccountDTO { Email = "test@example.com", Password = "ValidPass1!", Name = "Test User" };

            var result = await _controller.RegisterAccount(accountDTO);

            AssertStatusCode(202, result);
        }

        /// <summary>
        /// Deve retornar Created (201) quando o token for válido.
        /// </summary>
        [Fact]
        public async Task ConfirmAccount_ValidInput_ReturnsCreated()
        {
            string token = "UserFiveToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(201, result);
        }

        /// <summary>
        /// Deve retornar Gone (410) quando o token for expirado ou inutilizável.
        /// </summary>
        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsGone()
        {
            string token = "UserSixToken";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(410, result);
        }

        /// <summary>
        /// Deve retornar NotFound (404) quando o token não corresponder a nenhum usuário.
        /// </summary>
        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsNotFound()
        {
            string token = "UserFiveToken-Invalid";
            string hexToken = Convert.ToHexString(Encoding.UTF8.GetBytes(token));

            var result = await _controller.ConfirmAccount(hexToken);

            AssertStatusCode(404, result);
        }

        /// <summary>
        /// Deve retornar BadRequest (400) quando o token for nulo ou malformado.
        /// </summary>
        [Fact]
        public async Task ConfirmAccount_InvalidInput_ReturnsBadRequest()
        {
            var result = await _controller.ConfirmAccount(null);

            AssertStatusCode(400, result);
        }

        /// <summary>
        /// Deve retornar Accepted (202) ao enviar e-mail de recuperação para um usuário válido.
        /// </summary>
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

        /// <summary>
        /// Deve retornar NotFound (404) se o e-mail informado não estiver cadastrado.
        /// </summary>
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

        /// <summary>
        /// Deve retornar BadRequest (400) se o e-mail estiver ausente ou for nulo.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Conflict (409) se já houver um pedido de recuperação pendente para o e-mail.
        /// </summary>
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

        /// <summary>
        /// Deve retornar OK (200) ao redefinir a senha com token e senha válidos.
        /// </summary>
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

        /// <summary>
        /// Deve retornar NotFound (404) se o token informado não for encontrado.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Gone (410) se o token estiver expirado.
        /// </summary>
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

        /// <summary>
        /// Deve retornar BadRequest (400) se a nova senha não atender aos critérios de segurança.
        /// </summary>
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

        /// <summary>
        /// Deve retornar BadRequest (400) se o modelo estiver inválido (ex: email ausente).
        /// </summary>
        [Fact]
        public async Task LoginAccount_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var loginDTO = new LoginDTO { Password = "ValidPass1!" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(400, result);
        }

        /// <summary>
        /// Deve retornar Unauthorized (401) se as credenciais estiverem incorretas.
        /// </summary>
        [Fact]
        public async Task LoginAccount_InvalidCredentials_ReturnsUnauthorized()
        {
            var loginDTO = new LoginDTO { Email = "test@example.com", Password = "WrongPassword" };

            var result = await _controller.LoginAccount(loginDTO);

            AssertStatusCode(401, result);
        }

        /// <summary>
        /// Deve retornar Forbidden (403) se a conta estiver bloqueada.
        /// </summary>
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

        /// <summary>
        /// Deve retornar OK (200) se as credenciais estiverem corretas e retornar um token.
        /// </summary>
        [Fact]
        public async Task LoginAccount_ValidCredentials_ReturnsOk()
        {
            var loginDTO = new LoginDTO { Email = "User0@gmail.com", Password = "User0@gmail.com" };

            var result = await _controller.LoginAccount(loginDTO);

            var assertResult = AssertStatusCode(200, result);
            Assert.NotNull(assertResult.Value);
            Assert.Contains("Token", assertResult.Value.ToString());
        }

        /// <summary>
        /// Deve retornar Unauthorized (401) se o token fornecido for inválido.
        /// </summary>
        [Fact]
        public async Task LogoutAccount_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.LogoutAccount();

            AssertStatusCode(401, result);
        }

        /// <summary>
        /// Deve retornar OK (200) se o token for válido e o logout ocorrer com sucesso.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Unauthorized (401) se o token for inválido.
        /// </summary>
        [Fact]
        public async Task DeleteAccount_InvalidToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer awkjdbjahwbvdhgvawdhg";

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteAccount();

            AssertStatusCode(401, result);
        }

        /// <summary>
        /// Deve retornar Forbidden (403) se a conta estiver bloqueada.
        /// </summary>
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

        /// <summary>
        /// Deve retornar OK (200) ao excluir a conta com token válido.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Unauthorized (401) se não houver token na requisição.
        /// </summary>
        [Fact]
        public async Task ChangePassword_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var passwordDTO = new PasswordDTO { OldPassword = "2323", NewPassword = "NewPass2@" };

            var result = await _controller.ChangePassword(passwordDTO);

            AssertStatusCode(401, result);
        }

        /// <summary>
        /// Deve retornar Unauthorized (401) se o token fornecido for inválido.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Forbidden (403) se a conta estiver bloqueada.
        /// </summary>
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

        /// <summary>
        /// Deve retornar Unauthorized (401) se a senha atual estiver incorreta.
        /// </summary>
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

        /// <summary>
        /// Deve retornar BadRequest (400) se a nova senha não atender aos requisitos de segurança.
        /// </summary>
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

        /// <summary>
        /// Deve retornar OK (200) ao trocar a senha com sucesso, e permitir login com a nova senha.
        /// </summary>
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
