namespace TheGame.Common.Models
{
    public class SendGiftResponse
    {
        public int FromPlayerId { get; set; }

        public string ResourceType { get; set; }

        public double ResourceValue { get; set; }

        public double CurrentResourceBalance { get; set; }
    }
}