using System.Text.Json;
using MasteringAgentFramework.Ch07.ClaimsIntake;
using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

EnvLoader.LoadRepoRootEnv();

ClaimsTools tools = new();
IChatClient chatClient = AgentClientFactory.CreateChatClient();

AITool[] agentTools =
[
    AIFunctionFactory.Create(tools.LookUpPolicy),
    AIFunctionFactory.Create(tools.RecordClaimDetails),
    new ApprovalRequiredAIFunction(AIFunctionFactory.Create(tools.SubmitClaimForPayout))
];

const string instructions = """
You are a claims-intake assistant.
Record insurance claims and submit them for payout.
Use tools rather than guessing.
Do not invent policy or claim details.
Submitting a claim for payout is sensitive and requires human approval.
When the user asks to submit a claim for payout, call SubmitClaimForPayout; do not ask for approval in prose because the runtime asks the human before the tool executes.
Keep responses concise.
""";

AIAgent agent = chatClient.AsAIAgent(
        instructions,
        name: "ClaimsIntakeAssistant",
        tools: agentTools)
    .AsBuilder()
    .UseToolApproval()
    .Build();

string mode = args.Length > 0 ? args[0] : "scripted";

if (string.Equals(mode, "interactive", StringComparison.OrdinalIgnoreCase))
{
    await RunInteractiveAsync(agent);
}
else
{
    await RunScriptedAsync(agent);
}

static async Task RunScriptedAsync(AIAgent agent)
{
    Console.WriteLine("===== Path A: APPROVED =====");
    AgentSession approvedSession = await agent.CreateSessionAsync();
    await RunWithApprovalAsync(
        agent,
        approvedSession,
        "Look up POL-2201, record a home contents claim for a damaged desk lamp for 450 dollars, and submit it for payout.",
        _ => Task.FromResult(true));

    Console.WriteLine();
    Console.WriteLine("===== Path B: REJECTED =====");
    AgentSession rejectedSession = await agent.CreateSessionAsync();
    await RunWithApprovalAsync(
        agent,
        rejectedSession,
        "Record a travel claim on POL-2202 for delayed baggage for 300 dollars and ask to submit it for payout.",
        _ => Task.FromResult(false));
}

static async Task RunInteractiveAsync(AIAgent agent)
{
    AgentSession session = await agent.CreateSessionAsync();

    while (true)
    {
        Console.Write("You: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input) ||
            string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        await RunWithApprovalAsync(agent, session, input, PromptForApprovalAsync);
    }
}

static async Task RunWithApprovalAsync(
    AIAgent agent,
    AgentSession session,
    string userMessage,
    Func<ToolApprovalRequestContent, Task<bool>> approvalDecisionProvider)
{
    Console.WriteLine($"User: {userMessage}");
    AgentResponse response = await agent.RunAsync(userMessage, session);

    while (true)
    {
        List<ToolApprovalRequestContent> approvalRequests = response.Messages
            .SelectMany(message => message.Contents)
            .OfType<ToolApprovalRequestContent>()
            .ToList();

        if (approvalRequests.Count == 0)
        {
            Console.WriteLine("Final answer:");
            Console.WriteLine(response.Text);
            return;
        }

        List<AIContent> approvalResponses = [];

        foreach (ToolApprovalRequestContent request in approvalRequests)
        {
            PrintApprovalRequest(request);
            bool approved = await approvalDecisionProvider(request);
            Console.WriteLine($"Decision: {(approved ? "APPROVE" : "REJECT")}");

            approvalResponses.Add(request.CreateResponse(
                approved,
                approved ? "Approved by the human reviewer." : "Rejected by the human reviewer."));
        }

        ChatMessage approvalMessage = new(ChatRole.User, approvalResponses);
        response = await agent.RunAsync([approvalMessage], session);
    }
}

static Task<bool> PromptForApprovalAsync(ToolApprovalRequestContent request)
{
    Console.Write("Approve this action? (y/n): ");
    string? input = Console.ReadLine();
    return Task.FromResult(string.Equals(input, "y", StringComparison.OrdinalIgnoreCase));
}

static void PrintApprovalRequest(ToolApprovalRequestContent request)
{
    if (request.ToolCall is FunctionCallContent functionCall)
    {
        Console.WriteLine($"Approval required: {functionCall.Name}");
        Console.WriteLine($"Arguments: {JsonSerializer.Serialize(functionCall.Arguments)}");
        return;
    }

    Console.WriteLine($"Approval required: {request.ToolCall.GetType().Name}");
}
