using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProjectGaia.Server.Data;
using ProjectGaia.Server.Models;
using ProjectGaia.Server.Services;

namespace ProjectGaia.Server.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;

        public EventController(AppDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        // GET: Get All Account Events
        [HttpGet("")]
        public async Task<IActionResult> GetEvents()
        {
            var result = await _tokenService.GetAccount(_context, Request);
            Account? account = result.account;

            if (account == null) return StatusCodeResult(result.status);

            List<int> eventIDs = await _context.Events.AsNoTracking().Where(e => e.AccountID == account.ID).Select(e => e.ID).ToListAsync();

            return StatusCode(200, eventIDs);
        }

        // GET: Get Event
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

        // POST: Create Event
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

        // PUT: Edit Event
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

        // DELETE: Delete Event
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

        private ObjectResult StatusCodeResult((int code, string? message)? status)
        {
            return StatusCode(status?.code ?? 0, status?.message);
        }
    }
}
