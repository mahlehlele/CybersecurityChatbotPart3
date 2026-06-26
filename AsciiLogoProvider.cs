using System;
using System.IO;

namespace CybersecurityChatbot;

public static class AsciiLogoProvider
{
    public static string Load()
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ascii-art.txt");
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        return "[ Cybersecurity Awareness Chatbot ]";
    }
}
