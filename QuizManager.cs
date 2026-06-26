namespace CybersecurityChatbot;

public class QuizManager
{
    private readonly List<QuizQuestion> _questions;
    private readonly ActivityLogger _activityLogger;
    private readonly Random _random = new();
    private int _currentIndex;
    private int _score;
    private bool _started;
    private bool _currentQuestionAnswered;

    public QuizManager(ActivityLogger activityLogger)
    {
        _activityLogger = activityLogger;
        _questions = BuildQuestions();
    }

    public int CurrentNumber => _started ? Math.Min(_currentIndex + 1, _questions.Count) : 0;
    public int Score => _score;
    public int TotalQuestions => _questions.Count;
    public bool Started => _started;
    public bool CurrentQuestionAnswered => _currentQuestionAnswered;

    public void ResetQuiz()
    {
        _currentIndex = 0;
        _score = 0;
        _started = true;
        _currentQuestionAnswered = false;
        _activityLogger.Log("Quiz started.");
    }

    public QuizQuestion? GetCurrentQuestion()
    {
        if (!_started || IsFinished())
        {
            return null;
        }

        return _questions[_currentIndex];
    }

    public QuizAnswerResult SubmitAnswer(string answer)
    {
        QuizQuestion? question = GetCurrentQuestion();
        if (question is null)
        {
            return new QuizAnswerResult
            {
                Correct = false,
                Feedback = "Start the quiz first.",
                Explanation = string.Empty,
                Finished = true,
                Score = _score,
                Total = _questions.Count,
                FinalMessage = GetFinalMessage()
            };
        }

        if (_currentQuestionAnswered)
        {
            return new QuizAnswerResult
            {
                Correct = false,
                Feedback = "You already answered this question. Click Next Question.",
                Explanation = question.Explanation,
                Finished = IsFinished(),
                Score = _score,
                Total = _questions.Count,
                FinalMessage = string.Empty
            };
        }

        bool correct = NormaliseAnswer(answer) == NormaliseAnswer(question.CorrectAnswer);
        if (correct)
        {
            _score++;
        }

        _currentQuestionAnswered = true;
        string feedback = correct ? PickCorrectFeedback() : PickIncorrectFeedback(question.CorrectAnswer);
        _activityLogger.Log($"Quiz answer submitted - Question {_currentIndex + 1}: {(correct ? "correct" : "incorrect")}.");

        return new QuizAnswerResult
        {
            Correct = correct,
            Feedback = feedback,
            Explanation = question.Explanation,
            Finished = false,
            Score = _score,
            Total = _questions.Count,
            FinalMessage = string.Empty
        };
    }

    public bool MoveNext()
    {
        if (!_started)
        {
            return false;
        }

        if (!_currentQuestionAnswered)
        {
            return false;
        }

        _currentIndex++;
        _currentQuestionAnswered = false;

        if (IsFinished())
        {
            _activityLogger.Log($"Quiz completed - score: {_score} out of {_questions.Count}.");
            return false;
        }

        return true;
    }

    public bool IsFinished()
    {
        return _started && _currentIndex >= _questions.Count;
    }

    public string GetFinalScore()
    {
        return $"Final score: {_score} out of {_questions.Count}.";
    }

    public string GetFinalMessage()
    {
        double percent = _questions.Count == 0 ? 0 : (double)_score / _questions.Count;
        if (percent >= 0.85)
        {
            return "Great job! You're a cybersecurity pro. Keep using these habits every day.";
        }

        if (percent >= 0.6)
        {
            return "Good work. You understand many cyber safety basics. Review the explanations for the questions you missed.";
        }

        return "Keep learning to stay safe online. Focus on phishing, passwords, safe browsing, and social engineering.";
    }

    private string PickCorrectFeedback()
    {
        string[] responses =
        {
            "Correct! Nice cyber safety thinking.",
            "Correct! That is the safer choice.",
            "Well done, that answer protects users better.",
            "Correct. You spotted the safer option."
        };

        return responses[_random.Next(responses.Length)];
    }

    private string PickIncorrectFeedback(string correctAnswer)
    {
        string[] responses =
        {
            $"Not quite. The correct answer is {correctAnswer}.",
            $"Incorrect, but this is a useful learning moment. The correct answer is {correctAnswer}.",
            $"Almost there. The safer answer is {correctAnswer}.",
            $"That one is wrong. The correct answer is {correctAnswer}."
        };

        return responses[_random.Next(responses.Length)];
    }

    private static string NormaliseAnswer(string answer)
    {
        return answer.Trim().ToUpperInvariant()
            .Replace("A)", "A")
            .Replace("B)", "B")
            .Replace("C)", "C")
            .Replace("D)", "D")
            .Replace("TRUE", "TRUE")
            .Replace("FALSE", "FALSE");
    }

    private static List<QuizQuestion> BuildQuestions()
    {
        return new List<QuizQuestion>
        {
            new()
            {
                Question = "What should you do if you receive an email asking for your password?",
                Options = new List<string> { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" },
                CorrectAnswer = "C",
                Explanation = "Reporting phishing emails helps prevent scams and protects other users. Never reply with your password."
            },
            new()
            {
                Question = "True or False: It is safe to reuse one strong password on many websites.",
                Options = new List<string> { "True", "False" },
                CorrectAnswer = "False",
                IsTrueFalse = true,
                Explanation = "Reusing passwords is risky because one data breach can expose many of your accounts."
            },
            new()
            {
                Question = "Which password is strongest?",
                Options = new List<string> { "A) password123", "B) Your birthday", "C) A long unique passphrase", "D) Your pet's name" },
                CorrectAnswer = "C",
                Explanation = "Long unique passphrases are harder to guess and should be different for every account."
            },
            new()
            {
                Question = "What does HTTPS usually indicate in a website address?",
                Options = new List<string> { "A) The connection is encrypted", "B) The website is always safe", "C) The website is government owned", "D) The site needs no password" },
                CorrectAnswer = "A",
                Explanation = "HTTPS encrypts traffic, but you must still check the site is legitimate."
            },
            new()
            {
                Question = "True or False: Public Wi-Fi is always safe for online banking.",
                Options = new List<string> { "True", "False" },
                CorrectAnswer = "False",
                IsTrueFalse = true,
                Explanation = "Public Wi-Fi can be monitored. Avoid sensitive banking unless using a trusted connection."
            },
            new()
            {
                Question = "What is social engineering?",
                Options = new List<string> { "A) Fixing social media apps", "B) Manipulating people to reveal information", "C) Building networks", "D) Installing updates" },
                CorrectAnswer = "B",
                Explanation = "Social engineering tricks people into sharing sensitive information or taking unsafe actions."
            },
            new()
            {
                Question = "True or False: Two-factor authentication adds an extra layer of account protection.",
                Options = new List<string> { "True", "False" },
                CorrectAnswer = "True",
                IsTrueFalse = true,
                Explanation = "2FA requires a second proof, such as an authenticator code, making stolen passwords less useful."
            },
            new()
            {
                Question = "What is the safest response to a suspicious link in a message?",
                Options = new List<string> { "A) Click quickly", "B) Forward it to friends", "C) Verify through the official website or app", "D) Download the file first" },
                CorrectAnswer = "C",
                Explanation = "Verifying through official channels helps avoid phishing and malicious links."
            },
            new()
            {
                Question = "What should backups protect you against?",
                Options = new List<string> { "A) Ransomware and device failure", "B) Strong passwords", "C) Screen brightness", "D) Keyboard shortcuts" },
                CorrectAnswer = "A",
                Explanation = "Backups help restore files after ransomware, accidental deletion, or hardware failure."
            },
            new()
            {
                Question = "True or False: Antivirus software means you never need to update your computer.",
                Options = new List<string> { "True", "False" },
                CorrectAnswer = "False",
                IsTrueFalse = true,
                Explanation = "Updates patch security flaws. Antivirus is helpful but does not replace updates."
            },
            new()
            {
                Question = "Which action improves privacy on social media?",
                Options = new List<string> { "A) Share your location publicly", "B) Review privacy settings", "C) Accept every friend request", "D) Post ID documents" },
                CorrectAnswer = "B",
                Explanation = "Reviewing privacy settings limits who can see your personal information."
            },
            new()
            {
                Question = "What is ransomware?",
                Options = new List<string> { "A) Malware that locks files and demands payment", "B) A password manager", "C) A browser update", "D) A Wi-Fi setting" },
                CorrectAnswer = "A",
                Explanation = "Ransomware encrypts or locks files and demands payment. Backups and safe browsing reduce risk."
            },
            new()
            {
                Question = "True or False: You should share one-time passcodes with support agents who call you.",
                Options = new List<string> { "True", "False" },
                CorrectAnswer = "False",
                IsTrueFalse = true,
                Explanation = "Legitimate support staff should not ask for your one-time passcodes."
            },
            new()
            {
                Question = "Which is a safe way to install apps?",
                Options = new List<string> { "A) Random pop-up links", "B) Official app stores or trusted vendor sites", "C) Unknown email attachments", "D) Cracked software sites" },
                CorrectAnswer = "B",
                Explanation = "Official stores and trusted vendor sites reduce the risk of malware."
            },
            new()
            {
                Question = "What should you do before entering banking details online?",
                Options = new List<string> { "A) Check the URL and use the official app/site", "B) Use any link from SMS", "C) Disable 2FA", "D) Save details on public computers" },
                CorrectAnswer = "A",
                Explanation = "Always use official banking channels and check the address before entering details."
            }
        };
    }
}
