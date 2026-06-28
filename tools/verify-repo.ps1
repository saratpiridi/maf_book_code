Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

dotnet restore .\MasteringAgentFramework.sln
dotnet build .\MasteringAgentFramework.sln --no-restore
