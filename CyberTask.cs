namespace CybersecurityChatbot;

public class CyberTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reminder { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public string CreatedAt { get; set; } = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    public string CompletedAt { get; set; } = string.Empty;

    public string Status => IsComplete ? "Complete" : "Pending";
}
