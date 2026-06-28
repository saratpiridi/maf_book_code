# Chapter 7: Claims Intake

Chapter 7 adds human-in-the-loop approval. The agent can prepare a claim, but the
sensitive payout submission is wrapped in an approval-required function, so the
tool does not run until a human approves it.

The project has two modes:

- Scripted mode is the default and gives reproducible approved and rejected paths.
- Interactive mode lets you type a claim and approve or reject the tool call live.

Listings in this chapter:

- Listing 7-1: policy and claim records in `ClaimsData.cs`.
- Listing 7-2: `LookUpPolicy`, the read-only lookup tool.
- Listing 7-3: `RecordClaimDetails`, the draft-claim tool.
- Listing 7-4: `SubmitClaimForPayout`, the sensitive approval-required tool.
- Listing 7-5: wrapping the payout tool with `ApprovalRequiredAIFunction`.
- Listing 7-6: detecting `ToolApprovalRequestContent` and creating the response.
- Listing 7-7: scripted approved and rejected paths.
- Listing 7-8: interactive approval loop.

Scripted run from the repository root:

    dotnet run --project ./src/Ch07.ClaimsIntake/Ch07.ClaimsIntake.csproj

Interactive run from the repository root:

    dotnet run --project ./src/Ch07.ClaimsIntake/Ch07.ClaimsIntake.csproj -- interactive
