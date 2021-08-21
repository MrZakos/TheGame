using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TheGame.Common.Interfaces;

namespace TheGame.Server.Controllers
{
    /// <summary>
    /// WebSocket Controller - expose /connect route which allow websocket connections
    /// </summary>
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly IWebSocketHandler _websocketHandler;

        public WebSocketController(
            ILogger<WebSocketController> logger,
            IWebSocketHandler websocketHandler)
        {
            _logger = logger;
            _websocketHandler = websocketHandler;
        }

        [Route("/connect")]
        [HttpGet]
        public async Task Connect()
        {
            await _websocketHandler.HandleWebSocketRequestAsync(HttpContext);
        }
    }
}