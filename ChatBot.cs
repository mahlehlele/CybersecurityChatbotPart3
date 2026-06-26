using System.Text.RegularExpressions;

namespace CybersecurityChatbot;

public class ChatBot
{
    private delegate bool MessageRule(string rawInput, string normalisedInput, Sentiment sentiment, out string response);
    private delegate string ResponseFormatter(string response);

    private readonly KeywordResponder _keywordResponder;
    private readonly SentimentDetector _sentimentDetector;
    private readonly TaskManager _taskManager;
    private readonly QuizManager _quizManager;
    private readonly ActivityLogger _activityLogger;
    private readonly List<MessageRule> _rules;
    private readonly ResponseFormatter _responseFormatter;

    public ChatBot(TaskManager taskManager, QuizManager quizManager, ActivityLogger activityLogger)
    {
        _keywordResponder = new KeywordResponder();
        _sentimentDetector = new SentimentDetector();
        _taskManager = taskManager;
        _quizManager = quizManager;
        _activityLogger = activityLogger;
        _responseFormatter = AddPersonalisation;

        _rules = new List<MessageRule>
        {
            HandleHelpRequest,
            HandleMemoryRecall,
            HandleNameCapture,
            HandleInterestCapture,
            HandleFollowUp,
            HandleCybersecurityTopic,
            HandleGreeting,
            HandleThanks,
            HandleGoodbye
        };
    }

    public MemoryStore Memory { get; } = new();
    public string LastGuiAction { get; private set; } = string.Empty;

    public string GetWelcomeMessage()
    {
        return "Welcome to the Cybersecurity Awareness Chatbot POE. I can still answer Part 2 cybersecurity questions, remember your details, detect your mood, and now I can manage tasks, reminders, a quiz, and an activity log. What is your name?";
    }

    public string ProcessInput(string userInput)
    {
        LastGuiAction = string.Empty;
        string rawInput = userInput.Trim();
        string input = Normalise(rawInput);

        if (string.IsNullOrWhiteSpace(rawInput))
        {
            return "Please type a message first.";
        }

        if (rawInput.Length > 700)
        {
            return "That message is quite long. Please send one request at a time so I can help properly.";
        }

        Sentiment sentiment = _sentimentDetector.Detect(rawInput);

        // Part 3 NLP intent routing must happen before the normal Part 2 keyword flow.
        if (TryHandleNlpIntent(rawInput, input, out string nlpResponse))
        {
            CompleteTurn(sentiment);
            return nlpResponse;
        }

        foreach (MessageRule rule in _rules)
        {
            if (rule(rawInput, input, sentiment, out string response) && !string.IsNullOrWhiteSpace(response))
            {
                CompleteTurn(sentiment);
                return response;
            }
        }

        if (sentiment != Sentiment.Neutral)
        {
            string topicKey = !string.IsNullOrWhiteSpace(Memory.CurrentTopic) && _keywordResponder.TryFindTopic(Memory.CurrentTopic, out string currentTopic)
                ? currentTopic
                : "phishing";

            Memory.SetCurrentTopic(_keywordResponder.GetDisplayName(topicKey));
            string response = _sentimentDetector.GetResponsePrefix(sentiment) +
                              _keywordResponder.GetQuickTip(topicKey) +
                              " You can also ask me to add this as a task or reminder.";
            CompleteTurn(sentiment);
            return AddPersonalisation(response);
        }

        CompleteTurn(sentiment);
        return "I did not quite understand that. You can ask about passwords, phishing, scams, privacy, malware, 2FA, safe browsing, add a task, start the quiz, or show the activity log.";
    }

    public string GetMemorySummary()
    {
        return Memory.BuildSummary();
    }

    public string GetTopicList()
    {
        return _keywordResponder.GetTopicList();
    }

    private bool TryHandleNlpIntent(string rawInput, string input, out string response)
    {
        response = string.Empty;

        if (input.Contains("show more"))
        {
            _activityLogger.Log("Full activity log displayed through NLP command.");
            LastGuiAction = "OpenActivityLog";
            response = _activityLogger.GetFullLog();
            return true;
        }

        if (ContainsAny(input, "show activity log", "what have you done", "what did you do", "show log", "recent actions", "activity history"))
        {
            _activityLogger.Log($"NLP recognised log intent from: '{Shorten(rawInput)}'.");
            LastGuiAction = "OpenActivityLog";
            response = _activityLogger.GetRecentLog(10);
            return true;
        }

        if (ContainsAny(input, "start quiz", "take quiz", "test my knowledge", "quiz me", "play the game", "cyber quiz"))
        {
            _quizManager.ResetQuiz();
            _activityLogger.Log($"NLP recognised quiz intent from: '{Shorten(rawInput)}'.");
            LastGuiAction = "OpenQuiz";
            response = "The cybersecurity mini-game has started. I opened the Quiz tab for you. Answer one question at a time and I will give feedback after each answer.";
            return true;
        }

        if (ContainsAny(input, "show tasks", "view tasks", "my tasks", "task list", "show reminders", "view reminders"))
        {
            _activityLogger.Log($"NLP recognised show-tasks intent from: '{Shorten(rawInput)}'.");
            LastGuiAction = "OpenTasks";
            response = _taskManager.BuildTaskSummary();
            return true;
        }

        if (ContainsAny(input, "complete task", "mark complete", "mark as complete", "done with", "finished task"))
        {
            string title = ExtractTaskTitleForAction(input, new[] { "complete task", "mark complete", "mark as complete", "done with", "finished task" });
            CyberTask? task = _taskManager.FindTaskByTitle(title);
            if (task is null)
            {
                response = "I recognised that you want to complete a task, but I could not find it. Try: Complete task enable 2FA.";
                LastGuiAction = "OpenTasks";
                return true;
            }

            _taskManager.MarkAsComplete(task.Id);
            LastGuiAction = "RefreshTasks";
            response = $"Task marked complete: '{task.Title}'. Well done for improving your cyber safety.";
            return true;
        }

        if (ContainsAny(input, "delete task", "remove task", "cancel task", "discard task"))
        {
            string title = ExtractTaskTitleForAction(input, new[] { "delete task", "remove task", "cancel task", "discard task" });
            CyberTask? task = _taskManager.FindTaskByTitle(title);
            if (task is null)
            {
                response = "I recognised that you want to delete a task, but I could not find it. Try: Delete task review privacy settings.";
                LastGuiAction = "OpenTasks";
                return true;
            }

            _taskManager.DeleteTask(task.Id);
            LastGuiAction = "RefreshTasks";
            response = $"Task deleted: '{task.Title}'.";
            return true;
        }

        if (IsReminderReply(input))
        {
            string reminder = ExtractReminderText(rawInput);
            if (Memory.PendingReminderTaskId.HasValue)
            {
                _taskManager.SetReminder(Memory.PendingReminderTaskId.Value, reminder);
                string title = Memory.PendingReminderTaskTitle;
                Memory.ClearPendingReminderTask();
                LastGuiAction = "RefreshTasks";
                response = $"Got it! I'll remind you about '{title}' {reminder}.";
                return true;
            }
        }

        if (ContainsAny(input, "remind me to", "set reminder", "set a reminder", "reminder to", "don't forget", "dont forget"))
        {
            string title = ExtractReminderTaskTitle(rawInput);
            string reminder = ExtractReminderText(rawInput);
            if (string.IsNullOrWhiteSpace(title))
            {
                response = "I recognised a reminder request, but I need the task. For example: Remind me to update my password tomorrow.";
                return true;
            }

            CyberTask task = _taskManager.AddTask(Capitalise(title), $"Reminder task created from chat: {Capitalise(title)}.", reminder);
            _activityLogger.Log($"NLP recognised reminder intent from: '{Shorten(rawInput)}'.");
            LastGuiAction = "RefreshTasks";
            response = $"Reminder set for '{task.Title}' {reminder}.";
            return true;
        }

        if (ContainsAny(input, "add task", "add a task", "create task", "new task", "i need to", "set up", "enable 2fa", "enable two-factor"))
        {
            string title = ExtractTaskTitle(rawInput);
            string reminder = ContainsReminderPhrase(input) ? ExtractReminderText(rawInput) : string.Empty;
            if (string.IsNullOrWhiteSpace(title))
            {
                response = "I recognised that you want to add a task, but I need a task title. Try: Add task - Review privacy settings.";
                LastGuiAction = "OpenTasks";
                return true;
            }

            CyberTask task = _taskManager.AddTask(Capitalise(title), BuildAutoDescription(title), reminder);
            _activityLogger.Log($"NLP recognised task intent from: '{Shorten(rawInput)}'.");
            LastGuiAction = "RefreshTasks";

            if (string.IsNullOrWhiteSpace(reminder))
            {
                Memory.SetPendingReminderTask(task.Id, task.Title);
                response = $"Task added: '{task.Title}'. Would you like to set a reminder for this task? You can reply, 'Yes, remind me in 3 days'.";
            }
            else
            {
                response = $"Task added: '{task.Title}' with reminder {reminder}.";
            }

            return true;
        }

        return false;
    }

    private bool HandleHelpRequest(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "help", "what can you do", "commands", "options"))
        {
            return false;
        }

        response = "You can ask about cybersecurity topics, add tasks, set reminders, start the quiz, or view the activity log. Try: 'Add task - Review privacy settings', 'Remind me to update my password tomorrow', 'Quiz me', or 'Show activity log'.";
        return true;
    }

    private bool HandleMemoryRecall(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "what do you remember", "my name", "remember about me", "memory"))
        {
            return false;
        }

        response = Memory.BuildSummary();
        return true;
    }

    private bool HandleNameCapture(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        Match match = Regex.Match(rawInput, @"\b(?:my name is|i am|i'm|call me)\s+([A-Za-z][A-Za-z\-']*)", RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            return false;
        }

        string name = match.Groups[1].Value;
        if (IsCommonNonName(name))
        {
            return false;
        }

        Memory.SetName(name);
        response = $"Nice to meet you, {Memory.UserName}. I'll use your name while helping you stay safe online.";
        return true;
    }

    private bool HandleInterestCapture(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "interested in", "favourite topic", "favorite topic", "i like", "i want to learn about"))
        {
            return false;
        }

        if (_keywordResponder.TryFindTopic(input, out string topicKey))
        {
            string topic = _keywordResponder.GetDisplayName(topicKey);
            Memory.SetFavouriteTopic(topic);
            response = AddPersonalisation($"Great, I'll remember that you are interested in {topic}. {_keywordResponder.GetQuickTip(topicKey)}");
            return true;
        }

        response = "Tell me which cybersecurity topic interests you, such as privacy, phishing, passwords, malware, or 2FA.";
        return true;
    }

    private bool HandleFollowUp(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "tell me more", "explain more", "another tip", "more details", "continue", "what else"))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(Memory.CurrentTopic) || !_keywordResponder.TryFindTopic(Memory.CurrentTopic, out string topicKey))
        {
            response = "I can continue once we have a topic. Ask me about phishing, passwords, privacy, scams, malware, 2FA, safe browsing, or backups.";
            return true;
        }

        response = AddPersonalisation(_keywordResponder.GetMoreDetail(topicKey) + " " + _keywordResponder.GetQuickTip(topicKey));
        return true;
    }

    private bool HandleCybersecurityTopic(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!_keywordResponder.TryFindTopic(input, out string topicKey))
        {
            return false;
        }

        string displayName = _keywordResponder.GetDisplayName(topicKey);
        Memory.SetCurrentTopic(displayName);
        _activityLogger.Log($"Keyword matched: {displayName} - response delivered.");

        string prefix = _sentimentDetector.GetResponsePrefix(sentiment);
        response = AddPersonalisation(prefix + _keywordResponder.GetRandomResponse(topicKey) + " " + _keywordResponder.GetQuickTip(topicKey));
        return true;
    }

    private bool HandleGreeting(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "hello", "hi", "hey", "good morning", "good afternoon", "good evening"))
        {
            return false;
        }

        string name = string.IsNullOrWhiteSpace(Memory.UserName) ? "there" : Memory.UserName;
        response = $"Hello {name}. You can ask for a cyber tip, add a task, start the quiz, or show the activity log.";
        return true;
    }

    private bool HandleThanks(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "thank you", "thanks", "thank u"))
        {
            return false;
        }

        response = "You're welcome. Keep building safe online habits.";
        return true;
    }

    private bool HandleGoodbye(string rawInput, string input, Sentiment sentiment, out string response)
    {
        response = string.Empty;
        if (!ContainsAny(input, "bye", "goodbye", "exit", "quit"))
        {
            return false;
        }

        response = "Goodbye. Remember to keep passwords unique, check suspicious links, and complete your saved cyber tasks.";
        return true;
    }

    private void CompleteTurn(Sentiment sentiment)
    {
        Memory.CompleteTurn(sentiment);
    }

    private string AddPersonalisation(string response)
    {
        string prefix = string.IsNullOrWhiteSpace(Memory.UserName) ? string.Empty : $"{Memory.UserName}, ";
        string interest = string.IsNullOrWhiteSpace(Memory.FavouriteTopic)
            ? string.Empty
            : $" Since you are interested in {Memory.FavouriteTopic}, this is especially useful. ";
        return prefix + response + interest;
    }

    private static bool ContainsAny(string input, params string[] phrases)
    {
        return phrases.Any(input.Contains);
    }

    private static string Normalise(string value)
    {
        return value.ToLowerInvariant()
            .Replace("two-factor authentication", "2fa")
            .Replace("two factor authentication", "2fa")
            .Replace("multi-factor authentication", "mfa")
            .Replace("multi factor authentication", "mfa")
            .Trim();
    }

    private static string ExtractTaskTitle(string rawInput)
    {
        string text = rawInput.Trim();
        string[] markers =
        {
            "add task -", "add task:", "add task", "add a task to", "add a task", "create task to", "create task", "new task", "i need to", "set up", "enable"
        };

        foreach (string marker in markers)
        {
            int index = text.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                string title = text[(index + marker.Length)..].Trim(" .:-".ToCharArray());
                return RemoveReminderWords(title);
            }
        }

        return RemoveReminderWords(text);
    }

    private static string ExtractReminderTaskTitle(string rawInput)
    {
        string text = rawInput.Trim();
        Match match = Regex.Match(text, @"remind me to\s+(.+?)(?:\s+(tomorrow|today|tonight|next week|in \d+ days?|in \d+ weeks?|on \d{4}-\d{2}-\d{2}).*)?$", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return RemoveReminderWords(match.Groups[1].Value.Trim());
        }

        return RemoveReminderWords(text.Replace("reminder to", string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractReminderText(string rawInput)
    {
        string input = rawInput.Trim();
        string lower = input.ToLowerInvariant();

        Match inDays = Regex.Match(lower, @"in\s+(\d+)\s+days?");
        if (inDays.Success)
        {
            return $"in {inDays.Groups[1].Value} days";
        }

        Match inWeeks = Regex.Match(lower, @"in\s+(\d+)\s+weeks?");
        if (inWeeks.Success)
        {
            return $"in {inWeeks.Groups[1].Value} weeks";
        }

        Match date = Regex.Match(lower, @"\d{4}-\d{2}-\d{2}(?:\s+at\s+\d{1,2}:\d{2})?");
        if (date.Success)
        {
            return $"on {date.Value}";
        }

        if (lower.Contains("tomorrow"))
        {
            return "tomorrow";
        }

        if (lower.Contains("today") || lower.Contains("tonight"))
        {
            return "today";
        }

        if (lower.Contains("next week"))
        {
            return "next week";
        }

        return "soon";
    }

    private static string RemoveReminderWords(string title)
    {
        string cleaned = Regex.Replace(title, @"\b(in \d+ days?|in \d+ weeks?|tomorrow|today|tonight|next week|on \d{4}-\d{2}-\d{2}(?: at \d{1,2}:\d{2})?)\b", string.Empty, RegexOptions.IgnoreCase);
        return Regex.Replace(cleaned, @"\s+", " ").Trim(" .:-".ToCharArray());
    }

    private static string ExtractTaskTitleForAction(string input, IEnumerable<string> markers)
    {
        string title = input;
        foreach (string marker in markers)
        {
            int index = title.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return title[(index + marker.Length)..].Trim(" .:-".ToCharArray());
            }
        }

        return title.Trim();
    }

    private static bool ContainsReminderPhrase(string input)
    {
        return ContainsAny(input, "remind", "reminder", "tomorrow", "in 3 days", "in 5 days", "next week", "on 20");
    }

    private static bool IsReminderReply(string input)
    {
        return ContainsAny(input, "yes remind", "remind me", "set it", "set a reminder", "yes please") &&
               (input.Contains("day") || input.Contains("week") || input.Contains("tomorrow") || input.Contains("today") || input.Contains("next"));
    }

    private static string BuildAutoDescription(string title)
    {
        if (title.Contains("privacy", StringComparison.OrdinalIgnoreCase))
        {
            return "Review account privacy settings to ensure your data is protected.";
        }

        if (title.Contains("2fa", StringComparison.OrdinalIgnoreCase) || title.Contains("two", StringComparison.OrdinalIgnoreCase))
        {
            return "Enable two-factor authentication on important accounts.";
        }

        if (title.Contains("password", StringComparison.OrdinalIgnoreCase))
        {
            return "Update or strengthen password security for important accounts.";
        }

        return $"Cybersecurity task: {Capitalise(title)}.";
    }

    private static string Capitalise(string value)
    {
        string clean = value.Trim();
        if (string.IsNullOrWhiteSpace(clean))
        {
            return clean;
        }

        return char.ToUpper(clean[0]) + clean[1..];
    }

    private static bool IsCommonNonName(string value)
    {
        string lower = value.ToLowerInvariant();
        return lower is "worried" or "curious" or "frustrated" or "confused" or "interested" or "scared";
    }

    private static string Shorten(string text)
    {
        string clean = text.Replace(Environment.NewLine, " ").Trim();
        return clean.Length <= 80 ? clean : clean[..77] + "...";
    }
}
