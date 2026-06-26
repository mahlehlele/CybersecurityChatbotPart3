namespace CybersecurityChatbot;

public class QuizQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public bool IsTrueFalse { get; set; }
}

public class QuizAnswerResult
{
    public bool Correct { get; set; }
    public string Feedback { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public bool Finished { get; set; }
    public int Score { get; set; }
    public int Total { get; set; }
    public string FinalMessage { get; set; } = string.Empty;
}
