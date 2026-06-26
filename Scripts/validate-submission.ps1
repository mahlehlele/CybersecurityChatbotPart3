$ErrorActionPreference = "Stop"

$required = @(
  "CybersecurityChatbot.sln",
  "CybersecurityChatbot.csproj",
  "MainWindow.xaml",
  "MainWindow.xaml.cs",
  "ChatBot.cs",
  "KeywordResponder.cs",
  "SentimentDetector.cs",
  "MemoryStore.cs",
  "TaskManager.cs",
  "TaskStorageHelper.cs",
  "QuizManager.cs",
  "QuizQuestion.cs",
  "ActivityLogger.cs",
  "CyberTask.cs",
  "tasks.json",
  "Assets/greeting.wav",
  "Assets/ascii-art.txt",
  "Assets/cybersecurity-logo.png",
  ".github/workflows/build.yml",
  "README.md"
)

foreach ($file in $required) {
  if (-not (Test-Path $file)) {
    throw "Missing required file: $file"
  }
}

$csproj = Get-Content "CybersecurityChatbot.csproj" -Raw
if ($csproj -notmatch "UseWPF") { throw "Project is not configured for WPF." }
if ($csproj -notmatch "Newtonsoft.Json") { throw "Newtonsoft.Json package reference missing." }

$storage = Get-Content "TaskStorageHelper.cs" -Raw
if ($storage -notmatch "JsonConvert.SerializeObject") { throw "TaskStorageHelper does not serialize with Newtonsoft.Json." }
if ($storage -notmatch "JsonConvert.DeserializeObject") { throw "TaskStorageHelper does not deserialize with Newtonsoft.Json." }
if ($storage -notmatch "MarkAsComplete") { throw "MarkAsComplete method missing." }
if ($storage -notmatch "DeleteTask") { throw "DeleteTask method missing." }

$quiz = Get-Content "QuizManager.cs" -Raw
$questionCount = ([regex]::Matches($quiz, "Question = ")).Count
if ($questionCount -lt 10) { throw "Quiz has fewer than 10 questions." }

$xaml = Get-Content "MainWindow.xaml" -Raw
foreach ($tab in @("Task Assistant", "Cyber Quiz", "Activity Log")) {
  if ($xaml -notmatch [regex]::Escape($tab)) { throw "Missing GUI tab: $tab" }
}

if (Test-Path "bin") { throw "Remove bin folder before submission." }
if (Test-Path "obj") { throw "Remove obj folder before submission." }

Write-Host "Submission validation passed. Remember: commits, releases, GitHub Actions green tick, and YouTube video must still be done on GitHub/YouTube."
