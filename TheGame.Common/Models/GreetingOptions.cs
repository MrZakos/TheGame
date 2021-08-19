namespace TheGame.Common.Models
{
    public class GreetingOptions
    {
        public const string OptionsName = nameof(GreetingOptions);
        public string Title { get; set; }
        public string Name { get; set; }
        public int Loops { get; set; }
    }
}