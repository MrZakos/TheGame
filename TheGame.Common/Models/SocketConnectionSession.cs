using Microsoft.AspNetCore.Http;
using System;
using System.Net.WebSockets;

namespace TheGame.Common.Models
{
    public class SocketConnectionSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Player Player { get; set; }
        public WebSocket Socket { get; set; }
        public HttpContext HttpContext { get; set; }
        public DateTime ConnectedDateUTC { get; set; } = DateTime.UtcNow;
        public DateTime DisconnectedDateUTC { get; set; }
        public bool IsLoggedIn => Player != null;

        public override string ToString() => Player != null ? $"{Id} {Player}" :Id.ToString();        
    }
}