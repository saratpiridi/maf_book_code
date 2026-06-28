# Chapter 3: Providers And Identity

Chapter 3 keeps the same triage behavior from Chapter 2 but swaps the provider
path. The agent is created through the native Microsoft Foundry project endpoint,
which shows that the agent code is not tied to one client construction path.

Required `.env` values:

- `AZURE_AI_PROJECT_ENDPOINT`
- `AZURE_AI_MODEL_DEPLOYMENT_NAME`

Listings:

- Listing 3-3: `AgentClientFactory.CreateFoundryProjectAgent(...)` in `src/Shared`,
  building the agent via `AIProjectClient(...).AsAIAgent(...)`.
- Listing 3-4: `Program.cs` running the triage agent through the Foundry path.
- Listing 3-5: captured output from the Foundry project endpoint.

Run from the repository root:

    dotnet run --project ./src/Ch03.ProvidersAndIdentity/Ch03.ProvidersAndIdentity.csproj
