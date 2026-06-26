namespace CybersecurityChatbot;

public class MemoryStore
{
    private readonly List<string> _discussedTopics = new();

    public string UserName { get; private set; } = string.Empty;
    public string FavouriteTopic { get; private set; } = string.Empty;
    public string CurrentTopic { get; private set; } = string.Empty;
    public Sentiment LastSentiment { get; private set; } = Sentiment.Neutral;
    public int TurnCount { get; private set; }
    public int? PendingReminderTaskId { get; private set; }
    public string PendingReminderTaskTitle { get; private set; } = string.Empty;

    public IReadOnlyList<string> DiscussedTopics => _discussedTopics.AsReadOnly();

    public void SetName(string name)
    {
        UserName = CleanName(name);
    }

    public void SetFavouriteTopic(string topic)
    {
        FavouriteTopic = topic;
        SetCurrentTopic(topic);
    }

    public void SetCurrentTopic(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return;
        }

        CurrentTopic = topic.Trim();
        if (!_discussedTopics.Any(item => item.Equals(CurrentTopic, StringComparison.OrdinalIgnoreCase)))
        {
            _discussedTopics.Add(CurrentTopic);
        }
    }

    public void CompleteTurn(Sentiment sentiment)
    {
        TurnCount++;
        LastSentiment = sentiment;
    }

    public void SetPendingReminderTask(int id, string title)
    {
        PendingReminderTaskId = id;
        PendingReminderTaskTitle = title;
    }

    public void ClearPendingReminderTask()
    {
        PendingReminderTaskId = null;
        PendingReminderTaskTitle = string.Empty;
    }

    public string BuildSummary()
    {
        string name = string.IsNullOrWhiteSpace(UserName) ? "Not provided yet" : UserName;
        string favourite = string.IsNullOrWhiteSpace(FavouriteTopic) ? "Not set yet" : FavouriteTopic;
        string current = string.IsNullOrWhiteSpace(CurrentTopic) ? "No active topic" : CurrentTopic;
        string topics = _discussedTopics.Count == 0 ? "None yet" : string.Join(", ", _discussedTopics.TakeLast(6));
        string pendingReminder = PendingReminderTaskId.HasValue ? PendingReminderTaskTitle : "None";

        return $"Name: {name}\nFavourite topic: {favourite}\nCurrent topic: {current}\nRecent topics: {topics}\nMood: {LastSentiment}\nTurns: {TurnCount}\nPending reminder: {pendingReminder}";
    }

    private static string CleanName(string name)
    {
        string cleaned = name.Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return string.Empty;
        }

        return char.ToUpper(cleaned[0]) + cleaned[1..];
    }
}
