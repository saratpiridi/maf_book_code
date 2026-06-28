using Microsoft.Extensions.VectorData;

namespace MasteringAgentFramework.Ch06;

public static class HelpdeskDocuments
{
    public static IReadOnlyList<HelpdeskDocument> All { get; } =
    [
        new()
        {
            Id = "leave",
            SourceName = "Leave Policy",
            SourceLink = "kb://leave",
            Text = "Full-time employees accrue 20 days of paid annual leave per year. Up to 5 unused days may be carried into the following year and must be used by March 31, after which they are forfeited. Leave requests should be submitted at least two weeks in advance through the time-off system."
        },
        new()
        {
            Id = "expenses",
            SourceName = "Expense Reimbursement",
            SourceLink = "kb://expenses",
            Text = "Business expenses are reimbursed when submitted with an itemized receipt within 30 days. Client meals are reimbursable up to 75 dollars per person. Personal expenses, fines, and alcohol beyond a single drink are not reimbursable. Approved claims are paid in the next payroll cycle."
        },
        new()
        {
            Id = "it-access",
            SourceName = "Device Access",
            SourceLink = "kb://it-access",
            Text = "To request a replacement device, open a ticket with the IT service desk and select Hardware Replacement. Standard replacements arrive within three business days; loaner laptops are available same day for urgent needs. All devices must use full-disk encryption before storing company data."
        },
        new()
        {
            Id = "facilities",
            SourceName = "Facilities Access",
            SourceLink = "kb://facilities",
            Text = "Building access cards are issued by the facilities desk on the ground floor. A lost or stolen card must be reported immediately so it can be deactivated; a replacement is issued the same day for a 15 dollar fee. Visitors must be signed in and escorted at all times."
        },
        new()
        {
            Id = "security",
            SourceName = "Security Policy",
            SourceLink = "kb://security",
            Text = "Passwords must be at least 14 characters and are rotated every 90 days. Multi-factor authentication is required for all systems. Suspected phishing should be reported with the Report Phishing button and not forwarded. Accounts lock after five failed sign-in attempts."
        }
    ];
}

public sealed class HelpdeskDocument
{
    [VectorStoreKey]
    public string Id { get; init; } = string.Empty;

    [VectorStoreData]
    public string SourceName { get; init; } = string.Empty;

    [VectorStoreData]
    public string SourceLink { get; init; } = string.Empty;

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Text { get; init; } = string.Empty;

    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.CosineSimilarity)]
    public string EmbeddingSource => Text;
}
