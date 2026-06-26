namespace CybersecurityChatbot;

public class TaskManager
{
    private readonly TaskStorageHelper _storage;
    private readonly ActivityLogger _activityLogger;

    public TaskManager(TaskStorageHelper storage, ActivityLogger activityLogger)
    {
        _storage = storage;
        _activityLogger = activityLogger;
    }

    public string TaskFilePath => _storage.FilePath;

    public CyberTask AddTask(string title, string description, string reminder)
    {
        ValidateTask(title, description);
        CyberTask task = _storage.AddTask(title, description, reminder);
        string reminderText = string.IsNullOrWhiteSpace(task.Reminder)
            ? "no reminder set"
            : $"Reminder set for {task.Reminder}";
        _activityLogger.Log($"Task added: '{task.Title}' ({reminderText}).");
        return task;
    }

    public List<CyberTask> GetAllTasks()
    {
        return _storage.LoadTasks().OrderBy(task => task.IsComplete).ThenBy(task => task.Id).ToList();
    }

    public CyberTask? GetTaskById(int id)
    {
        return _storage.LoadTasks().FirstOrDefault(task => task.Id == id);
    }

    public CyberTask? FindTaskByTitle(string title)
    {
        string normalised = Normalise(title);
        return _storage.LoadTasks()
            .Where(task => Normalise(task.Title).Contains(normalised) || normalised.Contains(Normalise(task.Title)))
            .OrderBy(task => Math.Abs(task.Title.Length - title.Length))
            .FirstOrDefault();
    }

    public bool MarkAsComplete(int id)
    {
        CyberTask? task = GetTaskById(id);
        bool updated = _storage.MarkAsComplete(id);
        if (updated)
        {
            _activityLogger.Log($"Task marked complete: '{task?.Title ?? id.ToString()}'.");
        }

        return updated;
    }

    public bool DeleteTask(int id)
    {
        CyberTask? task = GetTaskById(id);
        bool deleted = _storage.DeleteTask(id);
        if (deleted)
        {
            _activityLogger.Log($"Task deleted: '{task?.Title ?? id.ToString()}'.");
        }

        return deleted;
    }

    public bool SetReminder(int id, string reminder)
    {
        CyberTask? task = GetTaskById(id);
        bool updated = _storage.UpdateReminder(id, reminder);
        if (updated)
        {
            _activityLogger.Log($"Reminder set: '{task?.Title ?? id.ToString()}' on {reminder}.");
        }

        return updated;
    }

    public string BuildTaskSummary()
    {
        List<CyberTask> tasks = GetAllTasks();
        if (tasks.Count == 0)
        {
            return "You do not have any cybersecurity tasks yet.";
        }

        List<string> lines = new() { "Here are your saved cybersecurity tasks:" };
        foreach (CyberTask task in tasks)
        {
            string reminder = string.IsNullOrWhiteSpace(task.Reminder) ? "No reminder" : task.Reminder;
            lines.Add($"{task.Id}. [{task.Status}] {task.Title} - {task.Description} (Reminder: {reminder})");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void ValidateTask(string title, string description)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Please enter a task title.");
        }

        if (title.Trim().Length < 3)
        {
            throw new ArgumentException("The task title is too short.");
        }

        if (title.Length > 120)
        {
            throw new ArgumentException("The task title must be shorter than 120 characters.");
        }

        if (description.Length > 600)
        {
            throw new ArgumentException("The task description must be shorter than 600 characters.");
        }
    }

    private static string Normalise(string value)
    {
        return value.ToLowerInvariant()
            .Replace("two-factor authentication", "2fa")
            .Replace("two factor authentication", "2fa")
            .Replace("multi factor authentication", "mfa")
            .Replace("multi-factor authentication", "mfa")
            .Trim();
    }
}
