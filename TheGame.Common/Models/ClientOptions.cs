namespace TheGame.Common.Models
{
    public class ClientOptions
    {
        public const string OptionsName = nameof(ClientOptions);
        public string WebSocketURL { get; set; }
        public int KeepAliveIntervalSeconds { get; set; }
    }
}