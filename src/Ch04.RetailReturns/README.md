# Chapter 4: Retail Returns

This is the first tools chapter. The agent can inspect an order, check return
eligibility, and create a return request, with the state-changing path guarded by
tool logic that re-checks eligibility before changing data.

Book listing map:

- Listing 4-1: `LookUpOrder`, a read-only function tool in `ReturnsData.cs`.
- Listing 4-2: creating the three tools and giving them to the agent in `Program.cs`.
- Listing 4-3: captured output, single-tool order lookup.
- Listing 4-4: captured output, two-tool eligible return.
- Listing 4-5: captured output, ineligible return refusal.
- Listing 4-6: `CreateReturnRequest` re-checking eligibility in `ReturnsData.cs`.

Run from the repository root:

    dotnet run --project ./src/Ch04.RetailReturns/Ch04.RetailReturns.csproj
