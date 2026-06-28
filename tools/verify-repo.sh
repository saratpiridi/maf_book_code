#!/usr/bin/env bash
set -euo pipefail

dotnet restore ./MasteringAgentFramework.sln
dotnet build ./MasteringAgentFramework.sln --no-restore
