
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using SmartSupport.Api.AI.Abstraction;
using SmartSupport.Api.AI.Documents;
using SmartSupport.Api.AI.Providers.Ollama;
using SmartSupport.Api.AI.Providers.OpenAI;
using SmartSupport.Api.AI.Tokenizer;
using SmartSupport.Api.Data;
using SmartSupport.Api.Options;
using SmartSupport.Api.Options.Ollama;
using SmartSupport.Api.Options.OpenAI;
using SmartSupport.Api.Services;
using SmartSupport.Api.Services.AI;
using SmartSupport.Api.Services.FileStorage;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<SmartSupportDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString));

builder.Services.AddHangfireServer();
builder.Services.AddOpenApi();

builder.Services.Configure<OllamaOptions>
    (builder.Configuration.GetSection(OllamaOptions.SectionName));

builder.Services.Configure<QdrantOptions>(
    builder.Configuration.GetSection(QdrantOptions.SectionName));

builder.Services.Configure<StorageOptions>(
    builder.Configuration.GetSection(StorageOptions.SectionName));

builder.Services.Configure<AIOptions>(
    builder.Configuration.GetSection(AIOptions.SectionName));

builder.Services.Configure<OpenAIOptions>(
    builder.Configuration.GetSection(OpenAIOptions.SectionName));


// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<PdfTextExtractor>();
builder.Services.AddScoped<TextChunker>();
builder.Services.AddScoped<IAiService, AiAssistantService>();
builder.Services.AddSingleton<TokenizerService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<DocumentIngestionService>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
builder.Services.AddScoped<IVectorSearchService, QdrantVectorSearchService>();
builder.Services.AddScoped<ILLMClientResolver,LLMClientResolver>();

builder.Services.AddSingleton<QdrantClient>(serviceProvider =>
{
    var options = serviceProvider
        .GetRequiredService<IOptions<QdrantOptions>>()
        .Value;

    return new QdrantClient(options.Host);
});
#region AI providers
#region Ollama
builder.Services.AddHttpClient<OllamaChatClient>((sp, client) =>
{
    var options = sp
        .GetRequiredService<IOptions<OllamaOptions>>()
        .Value;

    client.BaseAddress = new Uri(options.BaseUrl);
    client.Timeout = TimeSpan.FromMinutes(5);
});

// Register Ollama with Key
builder.Services.AddKeyedScoped<ILLMClient>(
    "Ollama",
    (sp, key) => sp.GetRequiredService<OllamaChatClient>());
#endregion
#region OpenAI
builder.Services.AddHttpClient<OpenAIChatClient>((sp, client) =>
{
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    client.Timeout = TimeSpan.FromMinutes(5);

    var options = sp
        .GetRequiredService<IOptions<OpenAIOptions>>()
        .Value;

    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Bearer",
            options.ApiKey);
});

builder.Services.AddKeyedScoped<ILLMClient>(
    "OpenAI",
    (sp, key) => sp.GetRequiredService<OpenAIChatClient>());
#endregion
#endregion

builder.Services
    .AddHttpClient<IEmbeddingClient, OllamaEmbeddingClient>((serviceProvider, client) =>
    {
        var options = serviceProvider
            .GetRequiredService<IOptions<OllamaOptions>>()
            .Value;

        client.BaseAddress = new Uri(options.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Enable Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHangfireDashboard("/hangfire/SmartSupportJob");
}

app.UseHttpsRedirection();
app.MapControllers();


//app.MapGet("/test-qdrant", async (QdrantClient client) =>
//{
//    var collections = await client.ListCollectionsAsync();

//    return Results.Ok(new
//    {
//        IsConnected = true,
//        Collections = collections
//    });
//});

app.Run();
