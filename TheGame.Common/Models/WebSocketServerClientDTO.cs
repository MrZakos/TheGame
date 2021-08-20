using System;

namespace TheGame.Common.Models
{
    public class WebSocketServerClientDTO
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public WebSocketServerClientEventCode? EventCode { get; set; }
        public string Event { get; set; }
        public bool? Success { get; set; }
        public string Message { get; set; }
        public LoginRequest LoginRequest { get; set; }
        public LoginResponse LoginResponse { get; set; }
        public UpdateResourcesRequest UpdateResourcesRequest { get; set; }
        public UpdateResourcesResponse UpdateResourcesResponse { get; set; }
        public SendGiftRequest SendGiftRequest { get; set; }
        public SendGiftResponse SendGiftResponse { get; set; }
    }
}