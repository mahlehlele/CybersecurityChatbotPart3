namespace CybersecurityChatbot;

public enum Sentiment
{
    Neutral,
    Worried,
    Curious,
    Frustrated,
    Confused
}

public class SentimentDetector
{
    private readonly Dictionary<Sentiment, string[]> _keywords = new()
    {
        [Sentiment.Worried] = new[] { "worried", "scared", "afraid", "anxious", "nervous", "concerned", "panic" },
        [Sentiment.Curious] = new[] { "curious", "interested", "learn", "explain", "what is", "how does", "why" },
        [Sentiment.Frustrated] = new[] { "frustrated", "angry", "annoyed", "stuck", "irritated", "fed up" },
        [Sentiment.Confused] = new[] { "confused", "lost", "unsure", "don't understand", "do not understand", "not sure" }
    };

    public Sentiment Detect(string input)
    {
        string lower = input.ToLowerInvariant();
        foreach (KeyValuePair<Sentiment, string[]> group in _keywords)
        {
            if (group.Value.Any(keyword => lower.Contains(keyword)))
            {
                return group.Key;
            }
        }

        return Sentiment.Neutral;
    }

    public string GetResponsePrefix(Sentiment sentiment)
    {
        return sentiment switch
        {
            Sentiment.Worried => "I understand why that feels worrying. Let's take it one safe step at a time. ",
            Sentiment.Curious => "Great question. Curiosity is one of the best ways to improve cyber safety. ",
            Sentiment.Frustrated => "That can definitely feel frustrating. The good news is that a few practical steps can reduce the risk. ",
            Sentiment.Confused => "It's okay to feel unsure. Cybersecurity terms can be confusing, so here is a simple explanation. ",
            _ => string.Empty
        };
    }
}
