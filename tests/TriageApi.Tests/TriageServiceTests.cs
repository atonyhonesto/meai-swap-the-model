using Microsoft.Extensions.AI;
using Xunit;

namespace TriageApi.Tests;

public class TriageServiceTests
{
    private const string CannedTriage = """
        {
          "Category": "Network",
          "Severity": "High",
          "Summary": "Indy office cannot reach the order portal while other sites can.",
          "SuggestedNextStep": "Check the Indy site's firewall and DNS changes since 9 a.m."
        }
        """;

    [Fact]
    public async Task TriageAsync_ReturnsTypedResult_FromModelJson()
    {
        var fake = new FakeChatClient(CannedTriage);
        var service = new TriageService(fake);

        var result = await service.TriageAsync("Indy office can't reach the order portal.");

        Assert.Equal("Network", result.Category);
        Assert.Equal("High", result.Severity);
        Assert.False(string.IsNullOrWhiteSpace(result.SuggestedNextStep));
    }

    [Fact]
    public async Task TriageAsync_SendsIncidentTextToModel()
    {
        var fake = new FakeChatClient(CannedTriage);
        var service = new TriageService(fake);
        const string incident = "Nightly EDI batch failed with a timeout.";

        await service.TriageAsync(incident);

        var sentText = string.Join("\n", fake.ReceivedMessages.Select(m => m.Text));
        Assert.Contains(incident, sentText);
    }

    [Fact]
    public async Task TriageAsync_RequestsJsonResponseFormat()
    {
        var fake = new FakeChatClient(CannedTriage);
        var service = new TriageService(fake);

        await service.TriageAsync("Checkout returns HTTP 500 intermittently.");

        // GetResponseAsync<T> asks the provider for schema-shaped JSON.
        Assert.IsType<ChatResponseFormatJson>(fake.ReceivedOptions?.ResponseFormat);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task TriageAsync_RejectsEmptyIncidentText(string text)
    {
        var service = new TriageService(new FakeChatClient(CannedTriage));

        await Assert.ThrowsAnyAsync<ArgumentException>(() => service.TriageAsync(text));
    }
}
