using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão das faturas de um utilizador.
    /// As operações incluem: obtenção de todas as faturas, obtenção de uma fatura específica, envio de faturas, edição e eliminação.
    /// </summary>
    [ApiController]
    [Route("api/invoices")]
    public class InvoiceController : ControllerBase
    {
        /// <summary>
        /// Contexto da base de dados utilizado para interagir com o banco de dados da aplicação.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Serviço de autenticação e verificação de token responsável por validar e obter informações sobre a conta do utilizador a partir do token do request.
        /// </summary>
        private readonly TokenService _tokenService;

        /// <summary>
        /// Construtor da classe InvoiceController. Injeta o contexto da base de dados e o serviço de tokens.
        /// </summary>
        /// <param name="context">O contexto da base de dados, utilizado para realizar operações de acesso e manipulação de dados.</param>
        /// <param name="tokenService">O serviço de autenticação, responsável por validar tokens e recuperar informações da conta associada ao utilizador autenticado.</param>
        public InvoiceController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Obtém todas as faturas associadas à conta do utilizador autenticado.
        /// </summary>
        /// <returns>Lista de IDs das faturas ou um código de erro, caso o utilizador não esteja autenticado.</returns>
        [HttpGet("")]
        public async Task<IActionResult> GetInvoices()
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            List<int> invoiceIDs = await _context.Invoices.AsNoTracking().Where(i => i.AccountID == account.ID).Select(i => i.ID).ToListAsync();

            return StatusCode(200, invoiceIDs);
        }

        /// <summary>
        /// Obtém uma fatura específica com base no ID, se pertencer à conta do utilizador autenticado.
        /// </summary>
        /// <param name="id">ID da fatura.</param>
        /// <returns>Fatura ou código de erro, caso não seja encontrada ou não pertença ao utilizador.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice(int id)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            Invoice? invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.ID == id);
            if (invoice == null) return StatusCode(404, "Invoice not found");

            if (invoice.AccountID != account.ID) return StatusCode(403, "Access denied. This invoice belongs to another user");

            return StatusCode(200, invoice);
        }

        /// <summary>
        /// Envia uma nova fatura para a base de dados, associando-a à conta do utilizador autenticado.
        /// </summary>
        /// <param name="invoiceDTO">Objeto contendo os dados da fatura a ser criada.</param>
        /// <returns>Resultado da criação da fatura ou erro caso os dados sejam inválidos ou o utilizador não esteja autenticado.</returns>
        [HttpPost("")]
        public async Task<IActionResult> UploadInvoice([FromBody] InvoiceDTO invoiceDTO)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            if (!ModelState.IsValid || invoiceDTO.Price == null || invoiceDTO.Consumption == null || invoiceDTO.EmissionDate == null) return StatusCode(400, ModelState);

            Invoice invoice = new Invoice
            {
                Price = invoiceDTO.Price.Value,
                Consumption = invoiceDTO.Consumption.Value,
                EmissionDate = invoiceDTO.EmissionDate.Value,
                UploadDate = DateTime.UtcNow,
                AccountID = account.ID
            };

            try
            {
                await _context.Invoices.AddAsync(invoice);
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, "Error converting data, price or consumption possibly too large");
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Try again");
            }

            await _context.SaveChangesAsync();

            return StatusCode(201, invoice);
        }

        /// <summary>
        /// Edita uma fatura existente, desde que pertença à conta do utilizador autenticado.
        /// </summary>
        /// <param name="id">ID da fatura a ser editada.</param>
        /// <param name="invoiceDTO">Objeto contendo os dados atualizados da fatura.</param>
        /// <returns>Resultado da edição da fatura ou erro caso não seja encontrada ou o utilizador não tenha permissão.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditInvoice(int id, [FromBody] InvoiceDTO invoiceDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _tokenService.GetAccount(_context, Request);
                Account? account = result.account;

                if (account == null) return StatusCodeResult(result.status);

                if (!ModelState.IsValid || invoiceDTO.Price == null || invoiceDTO.Consumption == null || invoiceDTO.EmissionDate == null) return StatusCode(400, ModelState);

                Invoice? invoice = await _context.Invoices.Where(i => i.ID == id).FirstOrDefaultAsync();

                if (invoice == null) return StatusCode(404, "Invoice not found");

                if (invoice.AccountID != account.ID) return StatusCode(403, "Invoice belongs to another user");

                invoice.Price = invoiceDTO.Price.Value;
                invoice.Consumption = invoiceDTO.Consumption.Value;
                invoice.EmissionDate = invoiceDTO.EmissionDate.Value;

                _context.Invoices.Update(invoice);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return StatusCode(204, null);
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error. Try again");
            }
        }

        /// <summary>
        /// Elimina uma fatura existente, desde que pertença à conta do utilizador autenticado.
        /// </summary>
        /// <param name="id">ID da fatura a ser eliminada.</param>
        /// <returns>Resultado da eliminação ou erro caso não seja encontrada ou o utilizador não tenha permissão.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Invoice? invoice = await _context.Invoices.AsNoTracking().FirstOrDefaultAsync(i => i.ID == id);
                if (invoice == null) return StatusCode(404, "Invoice not found");

                if (invoice.AccountID != account.ID) return StatusCode(403, "Access denied. This invoice belongs to another user");
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error. Try again");
            }

            return StatusCode(200, "Invoice and related data deleted successfully");
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
