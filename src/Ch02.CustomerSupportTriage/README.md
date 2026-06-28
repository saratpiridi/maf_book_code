# Chapter 2: Customer Support Triage

This is the first agent in the book: a small customer-support triage assistant
connected through the shared Azure OpenAI client. It is deliberately simple so you
can see the agent shape before tools, memory, or retrieval are added.

What to notice:

- Single-turn execution returns an `AgentResponse` and prints `response.Text`.
- Streaming uses `RunStreamingAsync` and the shared `ConsoleRenderer`.
- The conversation sample reuses one `AgentSession` across two turns.

Listing map:

- Listing 2-2: single-turn run.
- Listing 2-3: streaming response.
- Listing 2-4: two-turn conversation with session state.

Run from the repository root:

    dotnet run --project ./src/Ch02.CustomerSupportTriage/Ch02.CustomerSupportTriage.csproj
