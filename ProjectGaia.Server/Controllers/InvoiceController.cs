using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    [ApiController]
    [Route("invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public InvoiceController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // GET: Get Invoice
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            Invoice? invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.ID == id);
            if (invoice == null) return StatusCode(404, "Invoice not found");

            if (invoice.AccountID == id) return StatusCode(403, "Access denied. This invoice belongs to another user");

            return Ok(invoice);
        }

        // POST: Upload Invoice
        [HttpPost("upload")]
        public async Task<IActionResult> UploadInvoice([FromBody] InvoiceDTO invoiceDTO)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            Invoice invoice = new Invoice
            {
                Price = invoiceDTO.Price,
                Consumption = invoiceDTO.Consumption,
                EmissionDate = invoiceDTO.EmissionDate,
                UploadDate = DateTime.UtcNow,
                AccountID = account.ID
            };

            try
            {
                await _context.Invoice.AddAsync(invoice);
            }
            catch (DbUpdateException)
            {
                return StatusCodeResult((500, "Error converting data, price or consumption possibly too large"));
            }
            catch (Exception)
            {
                return StatusCodeResult((500, "Internal server error, try again"));
            }
            
            await _context.SaveChangesAsync();

            return Created();
        }

        private ObjectResult StatusCodeResult((int code, string? message)? status)
        {
            return StatusCode(status?.code ?? 0, status?.message);
        }
    }
}
