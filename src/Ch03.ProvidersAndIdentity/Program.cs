using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;

EnvLoader.LoadRepoRootEnv();

const string instructions = """
You triage customer-support messages for a generic product or service.
Identify the issue type, urgency, customer sentiment, and next best action.
Keep replies concise, practical, and limited to the support domain.
""";

AIAgent agent = AgentClientFactory.CreateFoundryProjectAgent(
    instructions,
    name: "TriageAgent");

AgentResponse response = await agent.RunAsync(
    "I was charged twice this month and need help before my renewal tomorrow.");

Console.WriteLine("Foundry project path");
Console.WriteLine(response.Text);
