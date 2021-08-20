using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using TheGame.BLL;

namespace TheGame.Server.Controllers
{
    /// <summary>
    /// WebSocketController - handles websocket initiation
    /// </summary>
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private readonly BusinessLogicLayer _bll;

        public WebSocketController(
            ILogger<WebSocketController> logger,
            BusinessLogicLayer businessLogicLayer)
        {
            _logger = logger;
            _bll = businessLogicLayer;
        }

        [Route("/connect")]
        [HttpGet]
        public async Task Connect()
        {
            await _bll.ProcessAcceptWebSocketRequestAsync(HttpContext);
        }
    }
}