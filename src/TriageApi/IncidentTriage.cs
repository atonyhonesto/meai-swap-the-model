namespace TriageApi;

/// <summary>
/// The structured result the model must return. Microsoft.Extensions.AI builds a
/// JSON schema from this record and deserializes the model's answer into it.
/// </summary>
public record IncidentTriage(
    string Category,          // e.g., "Network", "Database", "Authentication"
    string Severity,          // "Low", "Medium", "High", "Critical"
    string Summary,           // one-sentence plain-English summary
    string SuggestedNextStep);

/// <summary>Request body for POST /triage.</summary>
public record TriageRequest(string Text);
