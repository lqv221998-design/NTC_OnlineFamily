# PowerShell script to setup logic NTC.Core.Tests
$solutionDir = "D:\NTC_OnlineFamily\NTC_OnlineFamily"
$testDir = "$solutionDir\tests\NTC.Core.Tests"

Write-Host "Creating Test Project folder..."
New-Item -ItemType Directory -Force -Path $testDir | Out-Null

Write-Host "Generating NUnit Test Project..."
dotnet new nunit -n NTC.Core.Tests -o $testDir -f net8.0

Write-Host "Adding to Solution..."
dotnet sln "$solutionDir\NTC_OnlineFamily.sln" add "$testDir\NTC.Core.Tests.csproj"

Write-Host "Adding References..."
dotnet add "$testDir\NTC.Core.Tests.csproj" reference "$solutionDir\src\NTC.Core\NTC.Core.csproj"
dotnet add "$testDir\NTC.Core.Tests.csproj" package Newtonsoft.Json

Write-Host "Test Project Setup Complete."
