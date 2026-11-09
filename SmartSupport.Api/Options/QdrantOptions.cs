namespace SmartSupport.Api.Options
{
    public class QdrantOptions
    {
        public const string SectionName = "Qdrant";

        public string Host { get; set; } = "localhost";

        public string CollectionName { get; set; } = string.Empty;

    }
}
