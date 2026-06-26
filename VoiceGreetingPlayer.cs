using System;
using System.IO;
using System.Linq;
using System.Media;

namespace CybersecurityChatbot;

public class VoiceGreetingPlayer
{
    private SoundPlayer? _player;

    public string Play()
    {
        string? path = FindGreetingFile();

        if (path is null)
        {
            return "Voice greeting file not found. Expected Assets\\greeting.wav or Assets\\welcome.wav.";
        }

        try
        {
            // Stop any previous playback before starting again.
            _player?.Stop();
            _player?.Dispose();

            // Keep the SoundPlayer in a field so it is not disposed immediately.
            _player = new SoundPlayer(path);

            // Load checks that the WAV file is readable before trying to play it.
            _player.Load();

            // Play asynchronously so the GUI does not freeze.
            _player.Play();

            return $"Voice greeting playing: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            return $"Voice greeting could not play: {ex.Message}";
        }
    }

    private static string? FindGreetingFile()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string currentDirectory = Environment.CurrentDirectory;

        string[] possibleFiles =
        {
            Path.Combine(baseDirectory, "Assets", "greeting.wav"),
            Path.Combine(baseDirectory, "Assets", "welcome.wav"),
            Path.Combine(currentDirectory, "Assets", "greeting.wav"),
            Path.Combine(currentDirectory, "Assets", "welcome.wav")
        };

        return possibleFiles.FirstOrDefault(File.Exists);
    }
}
