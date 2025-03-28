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
    public class InvoiceControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly InvoiceController _controller;

        public InvoiceControllerTests(ITestOutputHelper testOutputHelper)
        {
            Environment.SetEnvironmentVariable("IS_UNIT_TEST", "-");

            _testOutputHelper = testOutputHelper;

            bool isGitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

            string connectionString = isGitHubActions ?
                $"Server=localhost,1433;Database=ProjectGaiaDBAccountTests;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};MultipleActiveResultSets=true" :
                "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBInvoiceTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new TestDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _tokenService = new TokenService();
            _controller = new InvoiceController(_context, _tokenService);
        }

        private ObjectResult AssertStatusCode(int statusCode, object? response)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(statusCode, assertResponse.StatusCode);
            return assertResponse;
        }

        [Fact]
        public async Task UploadInvoice_ValidInvoice_ReturnsCreated()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);

            AssertStatusCode(201, result);
        }

        [Fact]
        public async Task UploadInvoice_InvalidModel_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task UploadInvoice_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task UploadInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetInvoices_ValidToken_ReturnsInvoices()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoices();

            var resultObject = AssertStatusCode(200, result);

            var invoiceIDs = Assert.IsType<List<int>>(resultObject.Value);

            Assert.Single(invoiceIDs);
            Assert.Contains(1, invoiceIDs);
        }

        [Fact]
        public async Task GetInvoices_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoices();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetInvoices_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoices();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetInvoice_InvoiceExists_ReturnsInvoice()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(1);

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task GetInvoice_InvoiceDoesNotExist_ReturnsNotFound()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(69);

            AssertStatusCode(404, result);
        }

        [Fact]
        public async Task GetInvoice_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetInvoice_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(1);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task GetInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task EditInvoice_ValidInvoice_ReturnsNoContent()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 150, Consumption = 2000, EmissionDate = DateTime.UtcNow };
            var result = await _controller.EditInvoice(1, invoiceDTO);

            AssertStatusCode(204, result);
        }

        [Fact]
        public async Task EditInvoice_InvalidModel_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, EmissionDate = DateTime.UtcNow };
            var result = await _controller.EditInvoice(1, invoiceDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task EditInvoice_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.EditInvoice(1, invoiceDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task EditInvoice_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.EditInvoice(1, invoiceDTO);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task EditInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.EditInvoice(1, invoiceDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task DeleteInvoice_ValidToken_ReturnsOK()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteInvoice(1);

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task DeleteInvoice_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteInvoice(1);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task DeleteInvoice_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserOneToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteInvoice(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task DeleteInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteInvoice(1);

            AssertStatusCode(401, result);
        }
    }
}
