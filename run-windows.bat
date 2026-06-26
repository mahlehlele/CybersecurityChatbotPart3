@echo off
dotnet restore CybersecurityChatbot.sln
dotnet build CybersecurityChatbot.sln --configuration Release
dotnet run --project CybersecurityChatbot.csproj
pause
