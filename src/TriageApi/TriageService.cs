using Microsoft.Extensions.AI;

namespace TriageApi;

/// <summary>
/// The code that never changes. It depends only on <see cref="IChatClient"/> and has
/// no idea whether Ollama, Azure OpenAI, or Amazon Bedrock is answering.
/// </summary>
public class TriageService(IChatClient chatClient)
{
    public async Task<IncidentTriage> TriageAsync(string incidentText, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(incidentText);

        var response = await chatClient.GetResponseAsync<IncidentTriage>(
            $"""
            You are an IT operations triage assistant.
            Classify this incident and suggest one next step.
            Severity must be one of: Low, Medium, High, Critical.

            Incident:
            {incidentText}
            """,
            cancellationToken: cancellationToken);

        return response.Result;
    }
}
