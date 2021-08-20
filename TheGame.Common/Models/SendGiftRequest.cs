namespace TheGame.Common.Models
{
    public class SendGiftRequest
    {
        public int? FriendPlayerId { get; set; }

        public string ResourceType { get; set; }

        public double? ResourceValue { get; set; }
    }
}