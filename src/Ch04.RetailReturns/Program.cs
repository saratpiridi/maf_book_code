using MasteringAgentFramework.Ch04.RetailReturns;
using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

EnvLoader.LoadRepoRootEnv();

IChatClient chatClient = AgentClientFactory.CreateChatClient();

AITool[] tools =
[
    AIFunctionFactory.Create(ReturnsTools.LookUpOrder),
    AIFunctionFactory.Create(ReturnsTools.CheckReturnEligibility),
    AIFunctionFactory.Create(ReturnsTools.CreateReturnRequest)
];

const string instructions = """
You are a retail returns assistant.
Help a customer check an order, determine if an item can be returned, and start a return when eligible.
Use the tools rather than guessing.
Do not invent order or item details.
Do not promise a return unless it has been confirmed through the tools.
""";

AIAgent agent = chatClient.AsAIAgent(
    instructions,
    name: "ReturnsAssistant",
    tools: tools);

Console.WriteLine("Prompt 1: Order lookup");
AgentResponse lookupResponse = await agent.RunAsync(
    "What is in order ORD-1001?");
Console.WriteLine(lookupResponse.Text);
Console.WriteLine();

Console.WriteLine("Prompt 2: Eligible return");
AgentResponse eligibleReturnResponse = await agent.RunAsync(
    "I want to return item ITM-02 from order ORD-1001 because it is the wrong size. Can you check if it is eligible and start the return if it is?");
Console.WriteLine(eligibleReturnResponse.Text);
Console.WriteLine();

Console.WriteLine("Prompt 3: Ineligible return");
AgentResponse ineligibleReturnResponse = await agent.RunAsync(
    "I want to return item ITM-07 from order ORD-1003 because I changed my mind. Please start the return.");
Console.WriteLine(ineligibleReturnResponse.Text);
