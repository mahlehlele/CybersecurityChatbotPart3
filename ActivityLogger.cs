using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CybersecurityChatbot;

public class ActivityLogger
{
    private const string FileName = "activity-log.json";
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
    private readonly List<string> _log = new();

    public ActivityLogger()
    {
        LoadLog();
    }

    public event EventHandler? LogChanged;

    public void Log(string action)
    {
        string entry = DateTime.Now.ToString("[HH:mm] ") + action;
        _log.Add(entry);
        SaveLog();
        LogChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GetRecentLog(int count = 10)
    {
        if (_log.Count == 0)
        {
            return "No activity has been recorded yet.";
        }

        List<string> recent = _log.TakeLast(count).ToList();
        string formatted = FormatEntries(recent, "Here's a summary of recent actions:");
        if (_log.Count > count)
        {
            formatted += Environment.NewLine + Environment.NewLine + "Type 'show more' or click Show More to view the full activity history.";
        }

        return formatted;
    }

    public string GetFullLog()
    {
        if (_log.Count == 0)
        {
            return "No activity has been recorded yet.";
        }

        return FormatEntries(_log, "Full activity history:");
    }

    public int GetCount()
    {
        return _log.Count;
    }

    public IReadOnlyList<string> GetRecentEntries(int count = 10)
    {
        return _log.TakeLast(count).ToList();
    }

    public IReadOnlyList<string> GetAllEntries()
    {
        return _log.ToList();
    }

    private static string FormatEntries(IReadOnlyList<string> entries, string heading)
    {
        List<string> lines = new() { heading };
        for (int i = 0; i < entries.Count; i++)
        {
            lines.Add($"{i + 1}. {entries[i]}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private void LoadLog()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return;
            }

            string json = File.ReadAllText(_filePath);
            List<string>? items = JsonConvert.DeserializeObject<List<string>>(json);
            if (items is not null)
            {
                _log.Clear();
                _log.AddRange(items);
            }
        }
        catch
        {
            _log.Clear();
        }
    }

    private void SaveLog()
    {
        try
        {
            string json = JsonConvert.SerializeObject(_log, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // The app must continue running even if the log cannot be written.
        }
    }
}
