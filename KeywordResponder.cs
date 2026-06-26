namespace CybersecurityChatbot;

public class KeywordResponder
{
    private readonly Random _random = new();
    private readonly Dictionary<string, TopicResponseSet> _topics;
    private readonly Dictionary<string, int> _lastResponseIndexes = new();

    public KeywordResponder()
    {
        _topics = BuildTopics();
    }

    public bool TryFindTopic(string input, out string topicKey)
    {
        string lower = Normalise(input);
        foreach (KeyValuePair<string, TopicResponseSet> topic in _topics)
        {
            if (topic.Value.Keywords.Any(keyword => lower.Contains(keyword)))
            {
                topicKey = topic.Key;
                return true;
            }
        }

        topicKey = string.Empty;
        return false;
    }

    public string GetDisplayName(string topicKey)
    {
        return _topics.TryGetValue(topicKey, out TopicResponseSet? topic) ? topic.DisplayName : topicKey;
    }

    public string GetRandomResponse(string topicKey)
    {
        if (!_topics.TryGetValue(topicKey, out TopicResponseSet? topic))
        {
            return "I can help with passwords, phishing, privacy, scams, malware, and safe browsing.";
        }

        int index = PickWithoutImmediateRepeat(topicKey, topic.Responses.Count);
        return topic.Responses[index];
    }

    public string GetMoreDetail(string topicKey)
    {
        if (!_topics.TryGetValue(topicKey, out TopicResponseSet? topic))
        {
            return "Tell me which cybersecurity topic you want more detail about.";
        }

        return topic.MoreDetail;
    }

    public string GetQuickTip(string topicKey)
    {
        if (!_topics.TryGetValue(topicKey, out TopicResponseSet? topic))
        {
            return "Pause before clicking unexpected links and verify requests using official channels.";
        }

        return topic.PracticalTip;
    }

    public string GetTopicList()
    {
        return string.Join(", ", _topics.Values.Select(topic => topic.DisplayName));
    }

    private int PickWithoutImmediateRepeat(string topicKey, int count)
    {
        if (count <= 1)
        {
            return 0;
        }

        int previous = _lastResponseIndexes.TryGetValue(topicKey, out int last) ? last : -1;
        int next;
        do
        {
            next = _random.Next(count);
        } while (next == previous);

        _lastResponseIndexes[topicKey] = next;
        return next;
    }

    private static string Normalise(string value)
    {
        return value.ToLowerInvariant()
            .Replace("two-factor authentication", "2fa")
            .Replace("two factor authentication", "2fa")
            .Replace("multi-factor authentication", "mfa")
            .Replace("multi factor authentication", "mfa");
    }

    private static Dictionary<string, TopicResponseSet> BuildTopics()
    {
        return new Dictionary<string, TopicResponseSet>
        {
            ["password"] = new(
                "Password safety",
                new[] { "password", "passphrase", "login", "credential" },
                new[]
                {
                    "Use a long, unique password for every important account. A password manager can help you avoid reuse.",
                    "A strong password should be hard to guess and different from passwords used on other websites.",
                    "Avoid using your name, birthday, school, or pet name in a password because attackers can often find those details online."
                },
                "Password safety is about reducing the damage if one website is breached. Unique passwords stop one stolen password from opening every account.",
                "Create a unique passphrase for your email account first, because email is often used to reset other passwords."),
            ["phishing"] = new(
                "Phishing",
                new[] { "phishing", "fake email", "suspicious email", "suspicious link", "email asking", "link", "attachment" },
                new[]
                {
                    "Phishing messages try to make you rush. Check the sender, spelling, link destination, and whether the request makes sense.",
                    "Do not click unexpected links. Open the official website or app yourself instead of trusting a link in a message.",
                    "If an email asks for passwords, OTPs, or payment details, treat it as suspicious and report it."
                },
                "Phishing works by pretending to be a trusted person or company. The safest response is to verify through a separate official channel.",
                "Hover over links before clicking, and never enter passwords from a link you did not request."),
            ["scam"] = new(
                "Online scams",
                new[] { "scam", "fraud", "fake prize", "otp", "verification code", "money request" },
                new[]
                {
                    "Scammers often create urgency. Pause before sending money, codes, or personal information.",
                    "If someone asks for an OTP or verification code, do not share it. Those codes are meant only for you.",
                    "Check suspicious payment or prize messages with the organisation directly using its official contact details."
                },
                "Most scams rely on pressure, fear, or excitement. Slowing down and verifying the request can stop the attack.",
                "Do not share OTPs, banking PINs, or card details with anyone who contacts you unexpectedly."),
            ["privacy"] = new(
                "Privacy settings",
                new[] { "privacy", "personal information", "data", "settings", "profile", "location" },
                new[]
                {
                    "Review privacy settings on social media so only trusted people can see personal posts and contact details.",
                    "Be careful with location sharing. Public location posts can reveal where you live, study, or spend time.",
                    "Limit the personal information you post online because attackers can use it for guessing passwords or social engineering."
                },
                "Privacy protection means controlling what information others can see and how apps use your data.",
                "Check app permissions and remove access to your camera, microphone, or location when it is not needed."),
            ["malware"] = new(
                "Malware",
                new[] { "malware", "virus", "trojan", "spyware", "ransomware", "infected" },
                new[]
                {
                    "Malware can steal data, damage files, or spy on activity. Keep devices updated and avoid unknown downloads.",
                    "Only install software from official stores or trusted vendor websites to reduce malware risk.",
                    "If a device acts strangely, disconnect from the network and run a trusted security scan."
                },
                "Malware is harmful software. It often spreads through unsafe downloads, attachments, fake updates, and pirated programs.",
                "Avoid downloading cracked software because it is a common malware source."),
            ["2fa"] = new(
                "Two-factor authentication",
                new[] { "2fa", "mfa", "authenticator", "two step", "two-step", "multi factor", "multifactor" },
                new[]
                {
                    "Two-factor authentication adds a second proof of identity, making stolen passwords less useful.",
                    "Use an authenticator app where possible because it is usually safer than SMS codes.",
                    "Save backup codes in a safe place when enabling 2FA so you do not get locked out."
                },
                "2FA protects accounts by requiring something beyond the password, such as an app code, security key, or biometric approval.",
                "Enable 2FA on email, banking, cloud storage, and social media first."),
            ["safe browsing"] = new(
                "Safe browsing",
                new[] { "safe browsing", "browser", "https", "website", "url", "pop-up", "popup" },
                new[]
                {
                    "Check the website address carefully before entering passwords or banking information.",
                    "HTTPS protects the connection, but it does not automatically prove the website is honest.",
                    "Avoid pop-ups claiming your device is infected. Use your own security software instead."
                },
                "Safe browsing means checking URLs, avoiding suspicious downloads, and using official sites for sensitive activity.",
                "Type important website addresses yourself instead of clicking links in unexpected messages."),
            ["wifi"] = new(
                "Public Wi-Fi",
                new[] { "wifi", "wi-fi", "public network", "hotspot" },
                new[]
                {
                    "Avoid banking or entering sensitive passwords on public Wi-Fi unless you trust the network and use extra protection.",
                    "Turn off auto-join for public networks so your device does not connect without you noticing.",
                    "Use your mobile data for sensitive tasks when public Wi-Fi feels unsafe."
                },
                "Public Wi-Fi is convenient, but attackers can create fake hotspots or monitor insecure traffic.",
                "Forget public networks after using them so your device does not reconnect automatically."),
            ["social engineering"] = new(
                "Social engineering",
                new[] { "social engineering", "manipulate", "impersonate", "pretend", "urgent request" },
                new[]
                {
                    "Social engineering tricks people instead of hacking technology. Be careful with urgent or emotional requests.",
                    "If someone claims to be support, your bank, or your school, verify them through official channels before sharing information.",
                    "Attackers may use details from social media to sound believable, so limit what you share publicly."
                },
                "Social engineering succeeds when people feel rushed, scared, or too trusting. Verification is your defence.",
                "Call back using a number from the official website, not a number provided in the suspicious message."),
            ["backup"] = new(
                "Backups",
                new[] { "backup", "back up", "restore", "lost files", "external drive" },
                new[]
                {
                    "Backups protect you from ransomware, accidental deletion, and device failure.",
                    "Keep at least one backup separate from your main device so malware cannot easily damage it.",
                    "Test your backups occasionally. A backup only helps if you can restore from it."
                },
                "A good backup plan uses more than one copy and includes cloud or external storage.",
                "Back up important schoolwork and documents today, then set a reminder to repeat it weekly."),
            ["antivirus"] = new(
                "Antivirus protection",
                new[] { "antivirus", "anti-virus", "security scan", "defender", "scan" },
                new[]
                {
                    "Keep antivirus protection enabled and updated, but remember it does not replace safe behaviour.",
                    "Run a scan if you downloaded a suspicious file or your device suddenly behaves strangely.",
                    "Use trusted security tools only. Fake antivirus pop-ups can be scams."
                },
                "Antivirus tools detect many known threats, while updates and cautious browsing reduce your exposure to new ones.",
                "Schedule regular scans and keep real-time protection switched on.")
        };
    }

    private sealed class TopicResponseSet
    {
        public TopicResponseSet(string displayName, IEnumerable<string> keywords, IEnumerable<string> responses, string moreDetail, string practicalTip)
        {
            DisplayName = displayName;
            Keywords = keywords.Select(Normalise).ToList();
            Responses = responses.ToList();
            MoreDetail = moreDetail;
            PracticalTip = practicalTip;
        }

        public string DisplayName { get; }
        public List<string> Keywords { get; }
        public List<string> Responses { get; }
        public string MoreDetail { get; }
        public string PracticalTip { get; }
    }
}
