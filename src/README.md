# Source Projects

All source for the companion repository lives under this folder. The chapter
projects are intentionally small, so you can open one folder and see the complete
sample for that part of the book.

`Shared` holds the connection factory, `.env` loader, and console helpers reused
by the chapters. Model setup should stay there unless a chapter is explicitly
showing a different provider path.

Chapter projects follow the naming convention `Ch<NN>.<ScenarioName>`, for example
`Ch04.RetailReturns`. Everything builds under the single root solution, so one
`dotnet build` from the repository root covers the whole book.
