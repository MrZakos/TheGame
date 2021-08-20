namespace TheGame.Common.Models
{
    public class UpdateResourcesRequest
    {
        public string ResourceType { get; set; }

        public double? ResourceValue { get; set; }
    }
}