using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace MasteringAgentFramework.Ch05.AppointmentIntake;

internal sealed class IntakeMemoryProvider : AIContextProvider
{
    public string? ServiceType { get; private set; }

    public string? PreferredDate { get; private set; }

    public string? ContactNote { get; private set; }

    protected override ValueTask<AIContext> ProvideAIContextAsync(
        InvokingContext context,
        CancellationToken cancellationToken)
    {
        string missing = string.Join(", ", MissingDetails());
        string instructions = missing.Length == 0
            ? $"All intake details are present. Summarize them without claiming the appointment is booked. Service type: {ServiceType}; preferred date: {PreferredDate}; contact note: {ContactNote}."
            : $"Current intake memory: service type: {ServiceType ?? "missing"}; preferred date: {PreferredDate ?? "missing"}; contact note: {ContactNote ?? "missing"}. Ask only for these missing details: {missing}. Do not ask again for details already present.";

        return ValueTask.FromResult(new AIContext
        {
            Instructions = instructions
        });
    }

    protected override ValueTask StoreAIContextAsync(
        InvokedContext context,
        CancellationToken cancellationToken)
    {
        foreach (ChatMessage message in context.RequestMessages)
        {
            if (message.Role != ChatRole.User)
            {
                continue;
            }

            UpdateFromUserText(message.Text);
        }

        return ValueTask.CompletedTask;
    }

    public string DescribeState()
    {
        return $"Tracked fields: service type = {ServiceType ?? "missing"}; preferred date = {PreferredDate ?? "missing"}; contact note = {ContactNote ?? "missing"}";
    }

    private IEnumerable<string> MissingDetails()
    {
        if (ServiceType is null)
        {
            yield return "service type";
        }

        if (PreferredDate is null)
        {
            yield return "preferred date";
        }

        if (ContactNote is null)
        {
            yield return "contact note";
        }
    }

    private void UpdateFromUserText(string text)
    {
        string normalized = text.ToLowerInvariant();

        if (ServiceType is null)
        {
            if (normalized.Contains("haircut"))
            {
                ServiceType = "haircut";
            }
            else if (normalized.Contains("dental cleaning"))
            {
                ServiceType = "dental cleaning";
            }
            else if (normalized.Contains("appointment for "))
            {
                ServiceType = text[(normalized.IndexOf("appointment for ", StringComparison.Ordinal) + "appointment for ".Length)..].Trim().Trim('.');
            }
        }

        if (PreferredDate is null)
        {
            if (normalized.Contains("next tuesday"))
            {
                PreferredDate = "next Tuesday";
            }
            else if (normalized.Contains("friday afternoon"))
            {
                PreferredDate = "Friday afternoon";
            }
            else if (normalized.Contains("tomorrow"))
            {
                PreferredDate = "tomorrow";
            }
        }

        if (ContactNote is null)
        {
            const string notePrefix = "note:";
            int noteIndex = normalized.IndexOf(notePrefix, StringComparison.Ordinal);

            if (noteIndex >= 0)
            {
                ContactNote = text[(noteIndex + notePrefix.Length)..].Trim().Trim('.');
            }
            else if (normalized.Contains("call in the morning"))
            {
                ContactNote = "please call in the morning";
            }
            else if (normalized.Contains("text me to confirm"))
            {
                ContactNote = "text me to confirm";
            }
        }
    }
}
