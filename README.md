# Swap the Model, Keep the Code

![build](https://github.com/atonyhonesto/meai-swap-the-model/actions/workflows/build.yml/badge.svg)

One small .NET 10 app that runs against **three AI providers** (Ollama on your laptop, Azure OpenAI, and Amazon Bedrock) without changing a single line of the code that calls the model.

It's the companion repo to my LinkedIn article series on **Microsoft.Extensions.AI**, the standard AI abstraction layer for .NET.

## The idea in one sentence

Application code talks only to `IChatClient`. Provider types appear in exactly one place (startup), and configuration decides which one gets built.

```
TriageService ──► IChatClient ──► [cache] ──► [OpenTelemetry] ──► [logging] ──► ONE provider client
   (never changes)                         (same middleware for every provider)     Ollama | Azure | Bedrock
```

> You register **one** inner provider client and wrap it in middleware. You don't chain providers together; swapping providers means swapping that one center piece.

## What the app does

An IT incident triage API. Send it messy help-desk text; get back a strongly typed result:

```csharp
public record IncidentTriage(string Category, string Severity, string Summary, string SuggestedNextStep);
```

`GetResponseAsync<IncidentTriage>(...)` handles the JSON schema and deserialization, so there's no manual parsing.

## Repo layout

| Path | What's there |
|---|---|
| `src/TriageApi/Program.cs` | The provider switch and middleware pipeline: the only place providers appear |
| `src/TriageApi/TriageService.cs` | Provider-agnostic business logic: the code that never changes |
| `src/TriageApi/appsettings.json` | The `AI:Provider` switch |
| `src/TriageApi/TriageApi.http` | Ready-to-run requests for VS / VS Code / Rider |
| `samples/incidents.json` | Sample incidents for consistent comparisons across providers |
| `tests/TriageApi.Tests` | Unit tests using a fake `IChatClient`: no model, network, or cloud needed |
| `docs/results.md` | Side-by-side results from each provider |

## Quick start (local, free, no API keys)

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download) and [Ollama](https://ollama.com).

```bash
ollama pull llama3.2
dotnet run --project src/TriageApi
```

Then send `POST http://localhost:5080/triage` (see `TriageApi.http`), or:

```bash
curl -X POST http://localhost:5080/triage \
  -H "Content-Type: application/json" \
  -d '{"text":"Users in the Indy office cannot reach the order portal since 9 a.m. Other sites are OK."}'
```

## Switching providers

Change one value: `AI:Provider` in `appsettings.json`, or override it without editing files:

```bash
dotnet run --project src/TriageApi --AI:Provider=Azure
```

| Provider | Set these | Credentials (no keys in code) |
|---|---|---|
| `Ollama` | `AI:Ollama:Endpoint`, `AI:Ollama:Model` | None (local) |
| `Azure` | `AI:Azure:Endpoint`, `AI:Azure:Deployment` | `DefaultAzureCredential`: `az login` locally, managed identity in Azure. Needs the *Cognitive Services OpenAI User* role. |
| `Bedrock` | `AI:Bedrock:Region`, `AI:Bedrock:ModelId` | Standard AWS credential chain: `aws configure`, SSO, or an IAM role. Enable model access in the Bedrock console first. |

## Run the tests

```bash
dotnet test MeaiSwapTheModel.slnx
```

The tests swap in a `FakeChatClient`, which shows the second benefit of the abstraction: AI-dependent logic is unit-testable without calling a model.

## What "swappable" doesn't mean

- **Answers will differ.** Swapping providers is one line of code, not one line of testing. Re-evaluate whenever you swap.
- **Structured output isn't equally strict everywhere.** Providers with schema-constrained output enforce your shape; others fall back to prompt instructions. Validate either way.
- **Cost and speed vary widely.** Local is free per call but hardware-bound; cloud bills per token.
- **Watch what telemetry captures.** Logging prompts helps debugging and is risky with real data.

The abstraction protects your **code** from change. Protecting your **outcomes** still takes evaluation.

## Notes

- NuGet references use floating versions (`10.*`, `5.*`, and so on) so the demo stays current. Pin exact versions for anything beyond a demo.
- This is a learning sample, not a production service: no auth, no rate limiting, no input sanitization.

## License

MIT. See [LICENSE](LICENSE).

---

**Tony Honesto** · Cloud & Integration Engineer · [Read the first article](https://www.linkedin.com/pulse/net-microsoftextensionsai-tony-honesto-r4voc/) · [More repos](https://github.com/atonyhonesto)
