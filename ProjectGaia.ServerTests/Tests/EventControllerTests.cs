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
    public class EventControllerTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly AppDbContext _context;
        private readonly TokenService _tokenService;
        private readonly EventController _controller;

        public EventControllerTests(ITestOutputHelper testOutputHelper)
        {
            Environment.SetEnvironmentVariable("IS_UNIT_TEST", "-");

            _testOutputHelper = testOutputHelper;

            bool isGitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS") == "true";

            string connectionString = isGitHubActions ?
                $"Server=localhost,1433;Database=ProjectGaiaDBEventTests;User Id=sa;Password={Environment.GetEnvironmentVariable("SA_PASSWORD")};MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=False;" :
                "Server=(localdb)\\MSSQLLocalDB;Database=ProjectGaiaDBEventTests;Trusted_Connection=True;MultipleActiveResultSets=true";

            var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

            _context = new TestDbContext(options);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            _tokenService = new TokenService();
            _controller = new EventController(_context, _tokenService);
        }

        private ObjectResult AssertStatusCode(int statusCode, object? response)
        {
            ObjectResult assertResponse = Assert.IsAssignableFrom<ObjectResult>(response);
            Assert.Equal(statusCode, assertResponse.StatusCode);
            return assertResponse;
        }

        [Fact]
        public async Task CreateEvent_ValidEvent_ReturnsCreated()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.CreateEvent(eventDTO);

            AssertStatusCode(201, result);
        }

        [Fact]
        public async Task CreateEvent_InvalidModel_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.CreateEvent(eventDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task CreateEvent_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.CreateEvent(eventDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task CreateEvent_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.CreateEvent(eventDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetEvents_ValidToken_ReturnsEvents()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvents();

            var resultObject = AssertStatusCode(200, result);

            var eventIDs = Assert.IsType<List<int>>(resultObject.Value);

            Assert.Single(eventIDs);
            Assert.Contains(1, eventIDs);
        }

        [Fact]
        public async Task GetEvents_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvents();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetEvents_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvents();

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetEvent_EventExists_ReturnsEvent()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvent(1);

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task GetEvent_EventDoesNotExist_ReturnsNotFound()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvent(69);

            AssertStatusCode(404, result);
        }

        [Fact]
        public async Task GetEvent_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvent(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task GetEvent_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvent(1);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task GetEvent_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.GetEvent(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task EditEvent_ValidEvent_ReturnsNoContent()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.EditEvent(1, eventDTO);

            AssertStatusCode(204, result);
        }

        [Fact]
        public async Task EditEvent_InvalidModel_ReturnsBadRequest()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.EditEvent(1, eventDTO);

            AssertStatusCode(400, result);
        }

        [Fact]
        public async Task EditEvent_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserZeroToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.EditEvent(1, eventDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task EditEvent_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.EditEvent(1, eventDTO);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task EditEvent_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var eventDTO = new EventDTO { Name = "Pagamento 2", Description = "Descrição do evento", Date = new DateTime(2025, 3, 16), Type = EventType.Payment };
            var result = await _controller.EditEvent(1, eventDTO);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task DeleteEvent_ValidToken_ReturnsOK()
        {
            string token = "UserZeroToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteEvent(1);

            AssertStatusCode(200, result);
        }

        [Fact]
        public async Task DeleteEvent_WrongAccountToken_ReturnsForbidden()
        {
            string token = "UserOneToken";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteEvent(1);

            AssertStatusCode(403, result);
        }

        [Fact]
        public async Task DeleteEvent_InvalidToken_ReturnsUnauthorized()
        {
            string token = "UserOneToken-Invalid";
            string base64Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

            var mockHttpContext = new DefaultHttpContext();
            mockHttpContext.Request.Headers["Authorization"] = $"Bearer {base64Token}";
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteEvent(1);

            AssertStatusCode(401, result);
        }

        [Fact]
        public async Task DeleteEvent_NoToken_ReturnsUnauthorized()
        {
            var mockHttpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext };

            var result = await _controller.DeleteEvent(1);

            AssertStatusCode(401, result);
        }
    }
}
