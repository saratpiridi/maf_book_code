using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

EnvLoader.LoadRepoRootEnv();

IChatClient chatClient = AgentClientFactory.CreateChatClient();

const string instructions = """
You triage customer-support messages for a generic product or service.
Identify the issue type, urgency, customer sentiment, and next best action.
Keep replies concise, practical, and limited to the support domain.
""";

ChatClientAgent triageAgent = chatClient.AsAIAgent(instructions, name: "TriageAgent");
AIAgent agent = triageAgent;

// ===== Listing 2-2: Single turn =====
AgentResponse singleTurnResponse = await agent.RunAsync(
    "I was charged twice this month and need help before my renewal tomorrow.");

Console.WriteLine("Single turn");
Console.WriteLine(singleTurnResponse.Text);
Console.WriteLine();

// ===== Listing 2-3: Streaming =====
Console.WriteLine("Streaming");
IAsyncEnumerable<AgentResponseUpdate> streamingUpdates = agent.RunStreamingAsync(
    "My order arrived with a missing accessory, and I need it for setup today.");

await ConsoleRenderer.WriteStreamingResponseAsync(streamingUpdates);
Console.WriteLine();

// ===== Listing 2-4: Conversation =====
AgentSession session = await agent.CreateSessionAsync();

AgentResponse firstTurn = await agent.RunAsync(
    "I cannot reset my password because the reset link expired.",
    session);

Console.WriteLine("Conversation turn 1");
Console.WriteLine(firstTurn.Text);
Console.WriteLine();

AgentResponse secondTurn = await agent.RunAsync(
    "Also mention whether this should be treated as urgent.",
    session);

Console.WriteLine("Conversation turn 2");
Console.WriteLine(secondTurn.Text);
