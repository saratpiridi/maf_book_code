using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;

namespace MasteringAgentFramework.Shared;

public static class AgentClientFactory
{
    public static IChatClient CreateChatClient()
    {
        string endpoint = GetRequiredEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        string deployment = GetRequiredEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");

        AzureCliCredential credential = new();
        AzureOpenAIClientOptions options = new(AzureOpenAIClientOptions.ServiceVersion.V2024_10_21);
        AzureOpenAIClient azureClient = new(new Uri(endpoint), credential, options);
        return azureClient.GetChatClient(deployment).AsIChatClient();
    }

    public static AIAgent CreateFoundryProjectAgent(string instructions, string name)
    {
        string projectEndpoint = GetRequiredEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT");
        string deploymentName = GetRequiredEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME");

        AIAgent agent = new AIProjectClient(new Uri(projectEndpoint), new AzureCliCredential())
            .AsAIAgent(model: deploymentName, instructions: instructions, name: name);

        return agent;
    }

    private static string GetRequiredEnvironmentVariable(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Missing required environment variable '{name}'.");
        }

        return value;
    }
}
