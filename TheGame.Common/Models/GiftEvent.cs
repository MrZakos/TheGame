namespace TheGame.Common.Models
{
    public class GiftEvent
    {
        public int FromPlayerId { get; set; }

        public string ResourceType { get; set; }

        public double ResourceValue { get; set; }

        public double PreviousResourceBalance { get; set; }

        public double CurrentResourceBalance { get; set; }
    }
}