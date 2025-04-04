using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos eventos associados a uma conta de utilizador.
    /// Permite operações de leitura, criação, edição e exclusão de eventos.
    /// </summary>
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
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
        /// Construtor da classe. Injeta o contexto da base de dados e o serviço de tokens.
        /// </summary>
        /// <param name="context">O contexto da base de dados, utilizado para realizar operações de acesso e manipulação de dados.</param>
        /// <param name="tokenService">O serviço de autenticação, responsável por validar tokens e recuperar informações da conta associada ao utilizador autenticado.</param>
        public EventController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        /// <summary>
        /// Obtém os IDs de todos os eventos associados à conta do utilizador autenticado.
        /// </summary>
        /// <returns>Uma lista de IDs de eventos associados à conta do utilizador autenticado.</returns>
        [HttpGet("")]
        public async Task<IActionResult> GetEvents()
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            List<int> eventIDs = await _context.Events.AsNoTracking().Where(e => e.AccountID == account.ID).Select(e => e.ID).ToListAsync();

            return StatusCode(200, eventIDs);
        }

        /// <summary>
        /// Obtém um evento específico da conta do utilizador autenticado, com base no ID do evento.
        /// </summary>
        /// <param name="id">ID do evento a ser recuperado.</param>
        /// <returns>O evento correspondente ao ID, caso exista e pertença à conta do utilizador autenticado.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            Event? selectedEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.ID == id);
            if (selectedEvent == null) return StatusCode(404, "Event not found");

            if (selectedEvent.AccountID != account.ID) return StatusCode(403, "Access denied. This event belongs to another user");

            return StatusCode(200, selectedEvent);
        }

        /// <summary>
        /// Cria um novo evento associado à conta do utilizador autenticado.
        /// </summary>
        /// <param name="eventDTO">Objeto contendo as informações do evento a ser criado.</param>
        /// <returns>O evento criado, caso a operação seja bem-sucedida.</returns>
        [HttpPost("")]
        public async Task<IActionResult> CreateEvent([FromBody] EventDTO eventDTO)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            if (!ModelState.IsValid || eventDTO.Name == null || eventDTO.Description == null || eventDTO.Date == null || eventDTO.Type == null) return StatusCode(400, ModelState);

            Event newEvent = new Event
            {
                Name = eventDTO.Name,
                Description = eventDTO.Description,
                Date = eventDTO.Date.Value,
                Type = eventDTO.Type.Value,
                AccountID = account.ID
            };

            try
            {
                await _context.Events.AddAsync(newEvent);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error. Try again");
            }

            await _context.SaveChangesAsync();

            return StatusCode(201, newEvent);
        }

        /// <summary>
        /// Edita um evento existente associado à conta do utilizador autenticado, com base no ID do evento.
        /// </summary>
        /// <param name="id">ID do evento a ser editado.</param>
        /// <param name="eventDTO">Objeto contendo as informações atualizadas do evento.</param>
        /// <returns>Um código de status indicando o sucesso ou falha da operação.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> EditEvent(int id, [FromBody] EventDTO eventDTO)
        {
            using IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _tokenService.GetAccount(_context, Request);
                Account? account = result.account;

                if (account == null) return StatusCodeResult(result.status);

                if (!ModelState.IsValid || eventDTO.Name == null || eventDTO.Description == null || eventDTO.Date == null || eventDTO.Type == null) return StatusCode(400, ModelState);

                Event? currentEvent = await _context.Events.Where(e => e.ID == id).FirstOrDefaultAsync();

                if (currentEvent == null) return StatusCode(404, "Event not found");

                if (currentEvent.AccountID != account.ID) return StatusCode(403, "Event belongs to another user");

                currentEvent.Name = eventDTO.Name;
                currentEvent.Description = eventDTO.Description;
                currentEvent.Date = eventDTO.Date.Value;
                currentEvent.Type = eventDTO.Type.Value;

                _context.Events.Update(currentEvent);

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
        /// Exclui um evento existente associado à conta do utilizador autenticado, com base no ID do evento.
        /// </summary>
        /// <param name="id">ID do evento a ser excluído.</param>
        /// <returns>Um código de status indicando o sucesso ou falha da operação.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                Event? selectedEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(i => i.ID == id);
                if (selectedEvent == null) return StatusCode(404, "Event not found");

                if (selectedEvent.AccountID != account.ID) return StatusCode(403, "Access denied. This event belongs to another user");
                _context.Events.Remove(selectedEvent);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Internal server error. Try again");
            }

            return StatusCode(200, "Event deleted successfully");
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
