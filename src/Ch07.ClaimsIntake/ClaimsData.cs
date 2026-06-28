using System.ComponentModel;

namespace MasteringAgentFramework.Ch07.ClaimsIntake;

public sealed class ClaimsTools
{
    private readonly object _gate = new();
    private readonly Dictionary<string, ClaimRecord> _claims = [];
    private int _nextClaimNumber = 3001;

    [Description("Looks up an insurance policy by policy id and returns the coverage type and limit.")]
    public string LookUpPolicy(
        [Description("The policy id to look up, such as POL-2201.")]
        string policyId)
    {
        if (!ClaimsData.Policies.TryGetValue(policyId, out PolicyRecord? policy))
        {
            return $"Policy {policyId} was not found.";
        }

        return $"Policy {policy.PolicyId}: holder reference {policy.HolderReference}, coverage type {policy.CoverageType}, coverage limit {policy.CoverageLimit:C}.";
    }

    [Description("Records claim details as a draft claim after validating the policy and requested amount.")]
    public string RecordClaimDetails(
        [Description("The policy id for the claim.")]
        string policyId,
        [Description("The type of claim being recorded.")]
        string claimType,
        [Description("A short description of the loss.")]
        string description,
        [Description("The payout amount requested for the claim.")]
        decimal amountRequested)
    {
        if (!ClaimsData.Policies.ContainsKey(policyId))
        {
            return $"Cannot record the claim because policy {policyId} was not found.";
        }

        if (string.IsNullOrWhiteSpace(claimType))
        {
            return "Cannot record the claim because the claim type is missing.";
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return "Cannot record the claim because the loss description is missing.";
        }

        if (amountRequested <= 0)
        {
            return "Cannot record the claim because the requested amount must be greater than zero.";
        }

        lock (_gate)
        {
            string claimId = $"CLM-{_nextClaimNumber++}";
            _claims[claimId] = new ClaimRecord(
                claimId,
                policyId,
                claimType,
                description,
                amountRequested,
                ClaimStatus.Draft);

            return $"Draft claim {claimId} was recorded for policy {policyId} with requested amount {amountRequested:C}.";
        }
    }

    [Description("Submits a draft claim for payout after re-validating the policy limit.")]
    public string SubmitClaimForPayout(
        [Description("The claim id to submit for payout.")]
        string claimId)
    {
        lock (_gate)
        {
            if (!_claims.TryGetValue(claimId, out ClaimRecord? claim))
            {
                return $"Cannot submit claim {claimId} because it was not found.";
            }

            if (!ClaimsData.Policies.TryGetValue(claim.PolicyId, out PolicyRecord? policy))
            {
                _claims[claimId] = claim with { Status = ClaimStatus.Rejected };
                return $"Cannot submit claim {claimId} because policy {claim.PolicyId} was not found.";
            }

            if (claim.Status != ClaimStatus.Draft)
            {
                return $"Cannot submit claim {claimId} because its current status is {claim.Status}.";
            }

            if (claim.AmountRequested > policy.CoverageLimit)
            {
                _claims[claimId] = claim with { Status = ClaimStatus.Rejected };
                return $"Claim {claimId} was not submitted because the requested amount {claim.AmountRequested:C} exceeds the policy limit {policy.CoverageLimit:C}.";
            }

            _claims[claimId] = claim with { Status = ClaimStatus.Submitted };
            return $"Claim {claimId} was submitted for payout under policy {claim.PolicyId}.";
        }
    }
}

public static class ClaimsData
{
    public static IReadOnlyDictionary<string, PolicyRecord> Policies { get; } =
        new Dictionary<string, PolicyRecord>
        {
            ["POL-2201"] = new("POL-2201", "holder-ref-001", "home contents", 2500m),
            ["POL-2202"] = new("POL-2202", "holder-ref-002", "travel", 1200m),
            ["POL-2203"] = new("POL-2203", "holder-ref-003", "device protection", 900m)
        };
}

public sealed record PolicyRecord(
    string PolicyId,
    string HolderReference,
    string CoverageType,
    decimal CoverageLimit);

public sealed record ClaimRecord(
    string ClaimId,
    string PolicyId,
    string ClaimType,
    string Description,
    decimal AmountRequested,
    ClaimStatus Status);

public enum ClaimStatus
{
    Draft,
    Submitted,
    Rejected
}
