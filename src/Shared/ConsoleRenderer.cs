using Microsoft.Agents.AI;

namespace MasteringAgentFramework.Shared;

public static class ConsoleRenderer
{
    public static async Task WriteStreamingResponseAsync(
        IAsyncEnumerable<AgentResponseUpdate> updates,
        CancellationToken cancellationToken = default)
    {
        await foreach (AgentResponseUpdate update in updates.WithCancellation(cancellationToken))
        {
            WriteUpdate(update);
        }

        Console.WriteLine();
    }

    public static void WriteUpdate(AgentResponseUpdate update)
    {
        string text = update.Text;

        if (!string.IsNullOrEmpty(text))
        {
            Console.Write(text);
        }
    }
}
