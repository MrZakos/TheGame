using Newtonsoft.Json;
using System;

namespace TheGame.Common.Models
{

    public class WebSocketServerClientDTO
    {
        [JsonProperty("requestId")]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        [JsonProperty("eventCode")]
        public WebSocketServerClientEventCode? EventCode { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("success")]
        public bool? Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public LoginRequest LoginRequest { get; set; }

        [JsonProperty("data")]
        public LoginResponse LoginResponse { get; set; }

        [JsonProperty("data")]
        public UpdateResourcesRequest UpdateResourcesRequest { get; set; }

        [JsonProperty("data")]
        public UpdateResourcesResponse UpdateResourcesResponse { get; set; }

        [JsonProperty("data")]
        public SendGiftRequest SendGiftRequest { get; set; }

        [JsonProperty("data")]
        public SendGiftResponse SendGiftResponse { get; set; }
    }

    public enum WebSocketServerClientEventCode
    {
        Login,
        UpdateResources,
        SendGift,
        Message
    }

    public class LoginRequest
    {
        [JsonProperty("deviceId")]
        public Guid? DeviceId { get; set; }
    }

    public class LoginResponse
    {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }
    }

    public class UpdateResourcesRequest
    {
        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("resourceValue")]
        public double? ResourceValue { get; set; }
    }

    public class UpdateResourcesResponse
    {
        [JsonProperty("balance")]
        public double Balance { get; set; }
    }

    public class SendGiftRequest
    {
        [JsonProperty("friendPlayerId")]
        public int? FriendPlayerId { get; set; }

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("resourceValue")]
        public double? ResourceValue { get; set; }
    }

    public class SendGiftResponse
    {
        [JsonProperty("fromPlayerId")]
        public int? FromPlayerId { get; set; }

        [JsonProperty("resourceType")]
        public string ResourceType { get; set; }

        [JsonProperty("resourceValue")]
        public double? ResourceValue { get; set; }

        [JsonProperty("currentResourceBalance")]
        public double CurrentResourceBalance { get; set; }
    }
}