using Azure.AI.OpenAI;
using Azure.Identity;
using MasteringAgentFramework.Ch06;
using MasteringAgentFramework.Shared;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

#pragma warning disable MEAI001
#pragma warning disable SKEXP0001

EnvLoader.LoadRepoRootEnv();

IChatClient chatClient = AgentClientFactory.CreateChatClient();
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = CreateEmbeddingGenerator();

InMemoryVectorStore vectorStore = new(new InMemoryVectorStoreOptions
{
    EmbeddingGenerator = embeddingGenerator
});

VectorStoreCollection<string, HelpdeskDocument> collection =
    vectorStore.GetCollection<string, HelpdeskDocument>("workplace-helpdesk");

await collection.EnsureCollectionExistsAsync();
await collection.UpsertAsync(HelpdeskDocuments.All);

TextSearchProvider textSearchProvider = new(
    SearchDocumentsAsync,
    new TextSearchProviderOptions
    {
        SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
        CitationsPrompt = "Append a Sources section using each cited result's SourceName and SourceLink."
    });

const string instructions = """
You are an internal workplace-helpdesk assistant. Answer only from the retrieved documents and cite your sources. If the documents do not contain the answer, say you do not have that information.
""";

AIAgent agent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "WorkplaceHelpdeskRagAgent",
    ChatOptions = new ChatOptions
    {
        Instructions = instructions
    },
    AIContextProviders = [textSearchProvider]
});

AgentSession session = await agent.CreateSessionAsync();

await AskAsync(agent, session, "Q1", "How many unused vacation days can I carry into next year?");
await AskAsync(agent, session, "Q2", "What's the limit for a client dinner I can expense?");
await AskAsync(agent, session, "Q3", "How do I get a replacement laptop if mine stops working?");
await AskAsync(agent, session, "Q4", "I lost my building access card, what should I do?");
await AskAsync(agent, session, "Q5", "How do I enroll in the employee stock purchase plan?");

async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchDocumentsAsync(
    string query,
    CancellationToken cancellationToken)
{
    List<TextSearchProvider.TextSearchResult> results = [];

    await foreach (VectorSearchResult<HelpdeskDocument> result in collection.SearchAsync(
        query,
        top: 2,
        cancellationToken: cancellationToken))
    {
        results.Add(new TextSearchProvider.TextSearchResult
        {
            SourceName = result.Record.SourceName,
            SourceLink = result.Record.SourceLink,
            Text = result.Record.Text,
            RawRepresentation = result.Record
        });
    }

    return results;
}

static async Task AskAsync(AIAgent agent, AgentSession session, string label, string question)
{
    Console.WriteLine($"===== {label} =====");
    Console.WriteLine($"User: {question}");
    AgentResponse response = await agent.RunAsync(question, session);
    Console.WriteLine(response.Text);
    Console.WriteLine();
}

static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator()
{
    string endpoint = GetRequiredEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
    string embeddingDeployment = GetRequiredEnvironmentVariable("AZURE_OPENAI_EMBEDDING_DEPLOYMENT");

    AzureCliCredential credential = new();
    AzureOpenAIClientOptions options = new(AzureOpenAIClientOptions.ServiceVersion.V2024_10_21);
    AzureOpenAIClient azureOpenAIClient = new(new Uri(endpoint), credential, options);

    return azureOpenAIClient.GetEmbeddingClient(embeddingDeployment).AsIEmbeddingGenerator();
}

static string GetRequiredEnvironmentVariable(string name)
{
    string? value = Environment.GetEnvironmentVariable(name);

    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Missing required environment variable '{name}'.");
    }

    return value;
}
