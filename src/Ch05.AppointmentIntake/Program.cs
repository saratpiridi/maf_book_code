using System.Text.Json;
using MasteringAgentFramework.Ch05.AppointmentIntake;
using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

EnvLoader.LoadRepoRootEnv();

IChatClient chatClient = AgentClientFactory.CreateChatClient();

const string instructions = """
You are an appointment-intake assistant.
Collect three details for an appointment: service type, preferred date, and a short contact note.
Ask only for missing details.
Confirm back what has been collected so far.
Do not invent details or claim an appointment is booked.
""";

AIAgent agent = chatClient.AsAIAgent(instructions, name: "IntakeAssistant");

AgentSession levelOneSession = await RunLevel1SessionMemoryAsync(agent);
await RunLevel2PersistAndRestoreAsync(agent, levelOneSession);
await RunLevel3ContextProviderAsync(chatClient, instructions);

static async Task<AgentSession> RunLevel1SessionMemoryAsync(AIAgent agent)
{
    Console.WriteLine("===== Level 1: Session memory =====");

    AgentSession session = await agent.CreateSessionAsync();

    await RunTurnAsync(agent, session, "Turn 1", "I'd like to book an appointment for a dental cleaning.");
    await RunTurnAsync(agent, session, "Turn 2", "Sometime next Tuesday would work.");
    await RunTurnAsync(agent, session, "Turn 3", "What details do you still need from me?");

    Console.WriteLine();
    return session;
}

static async Task RunLevel2PersistAndRestoreAsync(AIAgent agent, AgentSession session)
{
    Console.WriteLine("===== Level 2: Persist and restore =====");

    JsonElement saved = await agent.SerializeSessionAsync(session);
    string statePath = Path.Combine(Directory.GetCurrentDirectory(), "src", "Ch05.AppointmentIntake", "session-state.json");

    await File.WriteAllTextAsync(statePath, saved.GetRawText());
    Console.WriteLine($"Saved session state to {statePath}");

    string restoredJson = await File.ReadAllTextAsync(statePath);
    JsonElement serialized = JsonSerializer.Deserialize<JsonElement>(restoredJson);
    AgentSession restored = await agent.DeserializeSessionAsync(serialized);

    await RunTurnAsync(agent, restored, "Restored turn", "My contact note is: please call in the morning.");

    Console.WriteLine();
}

static async Task RunLevel3ContextProviderAsync(IChatClient chatClient, string instructions)
{
    Console.WriteLine("===== Level 3: Context provider =====");

    IntakeMemoryProvider provider = new();

    AIAgent memoryAgent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "IntakeAssistant",
        ChatOptions = new ChatOptions
        {
            Instructions = instructions
        },
        AIContextProviders = [provider]
    });

    AgentSession session = await memoryAgent.CreateSessionAsync();

    await RunProviderTurnAsync(memoryAgent, session, provider, "Turn 1", "I need to schedule a haircut.");
    await RunProviderTurnAsync(memoryAgent, session, provider, "Turn 2", "Friday afternoon, please.");
    await RunProviderTurnAsync(memoryAgent, session, provider, "Turn 3", "Note: text me to confirm.");
}

static async Task RunProviderTurnAsync(
    AIAgent agent,
    AgentSession session,
    IntakeMemoryProvider provider,
    string label,
    string message)
{
    await RunTurnAsync(agent, session, label, message);
    Console.WriteLine(provider.DescribeState());
    Console.WriteLine();
}

static async Task RunTurnAsync(AIAgent agent, AgentSession session, string label, string message)
{
    Console.WriteLine(label);
    Console.WriteLine($"User: {message}");
    AgentResponse response = await agent.RunAsync(message, session);
    Console.WriteLine(response.Text);
    Console.WriteLine();
}
