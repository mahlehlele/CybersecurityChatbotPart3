namespace CybersecurityChatbot;

public class ChatMessage
{
    public ChatMessage(DateTime time, string speaker, string message)
    {
        Time = time;
        Speaker = speaker;
        Message = message;
    }

    public DateTime Time { get; }
    public string Speaker { get; }
    public string Message { get; }

    public override string ToString()
    {
        return $"[{Time:yyyy-MM-dd HH:mm}] {Speaker}: {Message}";
    }
}
