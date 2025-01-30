using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            _testOutputHelper = testOutputHelper;

            string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new AppDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _tokenService = new TokenService();
            _controller = new InvoiceController(_context, _tokenService);
        }

        [Fact]
        public async Task UploadInvoice_ValidInvoice_ReturnsCreated()
        {
            var account = new Account { ID = 1, Email = "test@example.com" };
            

            var invoiceDTO = new InvoiceDTO { Price = 100, Consumption = 200, EmissionDate = DateTime.UtcNow };
            var result = await _controller.UploadInvoice(invoiceDTO);
            var createdResult = Assert.IsType<CreatedResult>(result);

            Assert.NotNull(createdResult.Value);
        }

        [Fact]
        public async Task UploadInvoice_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Price", "Required");
            var invoiceDTO = new InvoiceDTO { Consumption = 200, EmissionDate = DateTime.UtcNow };

            var result = await _controller.UploadInvoice(invoiceDTO);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task GetInvoices_ValidAccount_ReturnsInvoices()
        {
            var account = new Account { ID = 1, Email = "test@example.com" };
            _context.Accounts.Add(account);
            _context.Invoices.Add(new Invoice { ID = 1, AccountID = 1, Price = 100, Consumption = 200 });
            await _context.SaveChangesAsync();

            

            var result = await _controller.GetInvoices();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var invoiceIds = Assert.IsType<List<int>>(okResult.Value);

            Assert.Single(invoiceIds);
            Assert.Contains(1, invoiceIds);
        }

        [Fact]
        public async Task GetInvoice_InvoiceExists_ReturnsInvoice()
        {
            var account = new Account { ID = 1, Email = "test@example.com" };
            var invoice = new Invoice { ID = 1, AccountID = 1, Price = 100, Consumption = 200 };
            _context.Accounts.Add(account);
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            


            var result = await _controller.GetInvoice(1);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedInvoice = Assert.IsType<Invoice>(okResult.Value);

            Assert.Equal(invoice.ID, returnedInvoice.ID);
        }

        [Fact]
        public async Task GetInvoice_InvoiceDoesNotExist_ReturnsNotFound()
        {
            var account = new Account { ID = 1, Email = "test@example.com" };
            


            var result = await _controller.GetInvoice(999);
            var notFoundResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
