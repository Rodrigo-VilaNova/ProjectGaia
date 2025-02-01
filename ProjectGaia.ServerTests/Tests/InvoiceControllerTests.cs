using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Environment.SetEnvironmentVariable("IS_UNIT_TEST", "");

            _testOutputHelper = testOutputHelper;

            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBInvoiceTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _tokenService = new TokenService();
            _controller = new InvoiceController(_context, _tokenService);
        }

        private ObjectResult AssertStatusCode(object? response, int statusCode)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(assertResponse.StatusCode, statusCode);
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

            AssertStatusCode(result, 201);
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

            AssertStatusCode(result, 400);
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

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task UploadInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);

            AssertStatusCode(result, 401);
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

            var resultObject = AssertStatusCode(result, 200);

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

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task GetInvoices_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoices();

            AssertStatusCode(result, 401);
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

            AssertStatusCode(result, 200);
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

            AssertStatusCode(result, 404);
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

            AssertStatusCode(result, 401);
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

            AssertStatusCode(result, 403);
        }

        [Fact]
        public async Task GetInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetInvoice(1);

            AssertStatusCode(result, 401);
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

            AssertStatusCode(result, 200);
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

            AssertStatusCode(result, 403);
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

            AssertStatusCode(result, 401);
        }

        [Fact]
        public async Task DeleteInvoice_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteInvoice(1);

            AssertStatusCode(result, 401);
        }
    }
}
