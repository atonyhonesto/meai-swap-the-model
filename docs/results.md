# Results: same code, three providers

Every run below used the **same build of the same code**. Only `AI:Provider` in
`appsettings.json` changed. Incidents come from [`samples/incidents.json`](../samples/incidents.json).

> Fill these in from your own runs. Disagreements between models are the
> interesting part, so keep them rather than "fixing" them.

## INC-001: Indy office cannot reach the order portal

| Provider | Model | Category | Severity | Response time | Approx. cost |
|---|---|---|---|---|---|
| Ollama (local) | llama3.2 | | | | $0 (local hardware) |
| Azure OpenAI | | | | | |
| Amazon Bedrock | | | | | |

## Observations

- Where did the models agree?
- Where did they disagree, and which answer would a senior engineer pick?
- Did any provider return JSON that needed a retry or failed validation?

## Test setup

- Date:
- Hardware (for local runs):
- Package versions (`dotnet list package`):
