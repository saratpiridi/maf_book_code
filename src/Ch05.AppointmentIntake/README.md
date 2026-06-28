# Chapter 5: Appointment Intake

This chapter teaches memory in three passes. The same appointment-intake assistant
first remembers within one session, then saves and restores the session, and then
uses a context provider to track the specific facts it still needs.

Running the sample writes `session-state.json` in this project folder. That file is
runtime output, not chapter source.

How the listings line up:

- Listing 5-1: Level 1 session memory with one `AgentSession` reused across turns.
- Listing 5-2: the first collected appointment details.
- Listing 5-3: Level 2 persistence with session JSON saved and restored.
- Listing 5-4: restored conversation output after loading `session-state.json`.
- Listing 5-5: `IntakeMemoryProvider`, a context provider for service type, preferred date, and contact note.
- Listing 5-6: attaching the context provider through `AIContextProviders`.
- Listing 5-7: Level 3 output showing the tracked fields after each turn.

Run from the repository root:

    dotnet run --project ./src/Ch05.AppointmentIntake/Ch05.AppointmentIntake.csproj
