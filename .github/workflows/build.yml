$ErrorActionPreference = "Stop"

Write-Host "Starting POE submission validation..."

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
if ($csproj -notmatch "UseWPF") {
  throw "Project is not configured for WPF."
}

if ($csproj -notmatch "Newtonsoft.Json") {
  throw "Newtonsoft.Json package reference missing."
}

$storage = Get-Content "TaskStorageHelper.cs" -Raw
if ($storage -notmatch "JsonConvert.SerializeObject") {
  throw "TaskStorageHelper does not serialize with Newtonsoft.Json."
}

if ($storage -notmatch "JsonConvert.DeserializeObject") {
  throw "TaskStorageHelper does not deserialize with Newtonsoft.Json."
}

if ($storage -notmatch "MarkAsComplete") {
  throw "MarkAsComplete method missing."
}

if ($storage -notmatch "DeleteTask") {
  throw "DeleteTask method missing."
}

$quiz = Get-Content "QuizManager.cs" -Raw
$questionCount = ([regex]::Matches($quiz, "Question = ")).Count
if ($questionCount -lt 10) {
  throw "Quiz has fewer than 10 questions."
}

$xaml = Get-Content "MainWindow.xaml" -Raw
foreach ($tab in @("Task Assistant", "Cyber Quiz", "Activity Log")) {
  if ($xaml -notmatch [regex]::Escape($tab)) {
    throw "Missing GUI tab: $tab"
  }
}

# Important:
# bin and obj are created automatically when dotnet build runs.
# GitHub Actions will create them during the build.
# So we must check whether bin/obj are COMMITTED to Git, not whether they exist after building.
$trackedFiles = @(git ls-files)
$badTrackedFiles = $trackedFiles | Where-Object {
  $_ -match '(^|/)(bin|obj)/'
}

if ($badTrackedFiles.Count -gt 0) {
  throw "bin/obj files are tracked in Git. Remove them from the repository: $($badTrackedFiles -join ', ')"
}

$readme = Get-Content "README.md" -Raw
if ($readme -match "ADD YOUR STUDENT NUMBER") {
  Write-Warning "README still contains student number placeholder."
}

if ($readme -match "ADD YOUR GITHUB REPOSITORY LINK") {
  Write-Warning "README still contains GitHub link placeholder."
}

if ($readme -match "ADD YOUR YOUTUBE VIDEO LINK") {
  Write-Warning "README still contains YouTube link placeholder."
}

Write-Host "Submission validation passed."
Write-Host "Remember: GitHub releases, green Actions tick, public repository, and unlisted YouTube video must still be completed."
