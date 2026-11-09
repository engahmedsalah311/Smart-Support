namespace SmartSupport.Api.Options.OpenAI
{
    public class OpenAIOptions
    {
        public const string SectionName = "OpenAI";

        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "gpt-6-luna";

    }
}
