using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace TheGame.Common.Interfaces
{
    public interface IWebSocketHandler
    {
        Task HandleWebSocketRequestAsync(HttpContext httpContext);
    }
}