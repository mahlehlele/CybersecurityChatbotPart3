# Cybersecurity Awareness Chatbot - Part 3 / POE

## Project description

This is a WPF GUI cybersecurity chatbot that combines Parts 1, 2, and 3 of the PROG6221 POE.

The application includes:

- Part 1 style voice greeting, cybersecurity branding, and ASCII art.
- Part 2 chatbot features: keyword recognition, random responses, conversation flow, memory, sentiment detection, and error handling.
- Part 3 Task Assistant with reminders using `tasks.json` JSON file storage.
- Part 3 Cybersecurity Mini-Game Quiz with more than 10 questions, one question at a time, immediate feedback, and a final score.
- Part 3 NLP simulation using keyword detection and string manipulation so the chatbot recognises differently worded task, reminder, quiz, and activity-log requests.
- Part 3 Activity Log that records key actions and displays the latest 10 entries with a Show More option.

## Prerequisites

- Windows computer
- Visual Studio 2022
- .NET 8.0 SDK
- Newtonsoft.Json NuGet package
- Git


```

## How to run

1. Open `CybersecurityChatbot.sln` in Visual Studio 2022.
2. Restore NuGet packages if prompted.
3. Build the solution.
4. Press **F5** to run.

Or use the terminal:

```powershell
dotnet restore CybersecurityChatbot.sln
dotnet build CybersecurityChatbot.sln --configuration Release
dotnet run --project CybersecurityChatbot.csproj
```

## JSON storage

The task assistant uses a JSON file named:

```text
tasks.json
```

The file is copied to the output folder and is also auto-created when the first task is added. No database server setup is required.

The task storage is handled in:

```text
TaskStorageHelper.cs
```

The business logic is handled in:

```text
TaskManager.cs
```

## Required files

```text
CybersecurityChatbot.sln
CybersecurityChatbot.csproj
MainWindow.xaml
MainWindow.xaml.cs
ChatBot.cs
KeywordResponder.cs
SentimentDetector.cs
MemoryStore.cs
TaskManager.cs
TaskStorageHelper.cs
QuizManager.cs
QuizQuestion.cs
ActivityLogger.cs
CyberTask.cs
tasks.json
Assets/greeting.wav
Assets/welcome.wav
Assets/ascii-art.txt
Assets/cybersecurity-logo.png
.github/workflows/build.yml
```

## Feature testing prompts

Use these prompts to demonstrate the application:

```text
My name is Asemahle
I am worried about phishing
tell me more
Give me a phishing tip
Give me a phishing tip
Add task - Review privacy settings
Yes, remind me in 3 days
Remind me to update my password tomorrow
Show tasks
Start quiz
Show activity log
show more
banana laptop cloud
```

## GitHub releases

Create these releases on GitHub:

- `v3.0` - JSON task assistant and reminders working
- `v3.1` - Quiz and activity log working
- `v3.2` - Final integrated POE version
