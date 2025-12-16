# PowerShell Script to Scaffold NTC_OnlineFamily Solution
# Run this from the root folder: d:\NTC_OnlineFamily\NTC_OnlineFamily

# 1. Create Solution
New-Item -ItemType Directory -Force -Path "src"
dotnet new sln -n NTC_OnlineFamily

# 2. Setup NTC.Core (.NET Standard 2.0)
# Note: specific Supabase nuget is incompatible with net48 in some versions, 
# so we rely on RestSharp for REST API access as implemented in SupabaseService.
dotnet new classlib -n NTC.Core -o "src\NTC.Core" -f netstandard2.0
dotnet sln add "src\NTC.Core\NTC.Core.csproj"
dotnet add "src\NTC.Core\NTC.Core.csproj" package Newtonsoft.Json
dotnet add "src\NTC.Core\NTC.Core.csproj" package RestSharp

# 3. Setup NTC.Revit (Multi-Targeting)
# We initially create it as wpf, then the csproj needs to be updated manually for multi-targeting
dotnet new wpf -n NTC.Revit -o "src\NTC.Revit"
dotnet sln add "src\NTC.Revit\NTC.Revit.csproj"
dotnet add "src\NTC.Revit\NTC.Revit.csproj" reference "src\NTC.Core\NTC.Core.csproj"
dotnet add "src\NTC.Revit\NTC.Revit.csproj" package MaterialDesignThemes

Write-Host "Scaffolding Complete. Ensure NTC.Revit.csproj is updated with TargetFrameworks."
