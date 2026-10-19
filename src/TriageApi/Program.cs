using Amazon;
using Amazon.BedrockRuntime;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OllamaSharp;
using TriageApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();

// ---------------------------------------------------------------------------
// The ONLY place providers appear. Configuration ("AI:Provider") decides which
// single inner client gets built. Everything after the switch is middleware
// that wraps that one client, identically for every provider.
// ---------------------------------------------------------------------------
builder.Services.AddChatClient(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>().GetSection("AI");

    return config["Provider"] switch
    {
        "Ollama" => new OllamaApiClient(
            new Uri(config["Ollama:Endpoint"]!), config["Ollama:Model"]!),

        // Uses DefaultAzureCredential: az login locally, managed identity in Azure.
        "Azure" => new AzureOpenAIClient(
                new Uri(config["Azure:Endpoint"]!), new DefaultAzureCredential())
            .GetChatClient(config["Azure:Deployment"]!)
            .AsIChatClient(),

        // Uses the standard AWS credential chain: aws configure / SSO / IAM role.
        "Bedrock" => new AmazonBedrockRuntimeClient(
                RegionEndpoint.GetBySystemName(config["Bedrock:Region"]!))
            .AsIChatClient(config["Bedrock:ModelId"]!),

        var p => throw new InvalidOperationException($"Unknown AI provider: '{p}'. Use Ollama, Azure, or Bedrock.")
    };
})
.UseDistributedCache()   // outermost: identical requests never reach the model
.UseOpenTelemetry()      // traces every real model call
.UseLogging();

builder.Services.AddScoped<TriageService>();

var app = builder.Build();

app.MapGet("/", (IConfiguration config) => Results.Ok(new
{
    service = "meai-swap-the-model",
    provider = config["AI:Provider"]
}));

app.MapPost("/triage", async (TriageRequest request, TriageService triage, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Text))
        return Results.BadRequest("Incident text is required.");

    var result = await triage.TriageAsync(request.Text, ct);
    return Results.Ok(result);
});

app.Run();

// Exposed for integration testing.
public partial class Program;
