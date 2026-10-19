using Microsoft.Extensions.AI;

namespace TriageApi.Tests;

/// <summary>
/// A stand-in for any real provider. Because TriageService depends only on
/// IChatClient, it can be tested without calling a model, a network, or a cloud.
/// </summary>
public sealed class FakeChatClient(string cannedJson) : IChatClient
{
    public List<ChatMessage> ReceivedMessages { get; } = [];
    public ChatOptions? ReceivedOptions { get; private set; }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ReceivedMessages.AddRange(messages);
        ReceivedOptions = options;
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, cannedJson)));
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("Streaming is not used by TriageService.");

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
