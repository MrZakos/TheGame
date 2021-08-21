namespace TheGame.Common.Models
{
    public class SendGiftResponse
    { 
        public string Message { get; set; }   
    }

    public class GiftEvent
    {
        public int FromPlayerId { get; set; }

        public string ResourceType { get; set; }

        public double ResourceValue { get; set; }

        public double PreviousResourceBalance { get; set; }

        public double CurrentResourceBalance { get; set; }
    }
}