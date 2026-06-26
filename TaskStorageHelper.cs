using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace CybersecurityChatbot;

// Attribution: Newtonsoft.Json - https://www.newtonsoft.com/json
// NuGet: Install-Package Newtonsoft.Json
public class TaskStorageHelper
{
    private const string FileName = "tasks.json";
    private readonly string _filePath;

    public TaskStorageHelper()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FileName);
    }

    public string FilePath => _filePath;

    public List<CyberTask> LoadTasks()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                SaveTasks(new List<CyberTask>());
                return new List<CyberTask>();
            }

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<CyberTask>();
            }

            return JsonConvert.DeserializeObject<List<CyberTask>>(json) ?? new List<CyberTask>();
        }
        catch
        {
            return new List<CyberTask>();
        }
    }

    public void SaveTasks(List<CyberTask> tasks)
    {
        try
        {
            string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not save tasks to {_filePath}: {ex.Message}", ex);
        }
    }

    public CyberTask AddTask(string title, string description, string reminder)
    {
        List<CyberTask> tasks = LoadTasks();
        int newId = tasks.Count == 0 ? 1 : tasks.Max(task => task.Id) + 1;

        CyberTask task = new()
        {
            Id = newId,
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description)
                ? $"Cybersecurity task: {title.Trim()}"
                : description.Trim(),
            Reminder = reminder.Trim(),
            IsComplete = false,
            CreatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            CompletedAt = string.Empty
        };

        tasks.Add(task);
        SaveTasks(tasks);
        return task;
    }

    public bool MarkAsComplete(int id)
    {
        List<CyberTask> tasks = LoadTasks();
        CyberTask? task = tasks.FirstOrDefault(item => item.Id == id);
        if (task is null)
        {
            return false;
        }

        task.IsComplete = true;
        task.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        SaveTasks(tasks);
        return true;
    }

    public bool DeleteTask(int id)
    {
        List<CyberTask> tasks = LoadTasks();
        CyberTask? task = tasks.FirstOrDefault(item => item.Id == id);
        if (task is null)
        {
            return false;
        }

        tasks.Remove(task);
        SaveTasks(tasks);
        return true;
    }

    public bool UpdateReminder(int id, string reminder)
    {
        List<CyberTask> tasks = LoadTasks();
        CyberTask? task = tasks.FirstOrDefault(item => item.Id == id);
        if (task is null)
        {
            return false;
        }

        task.Reminder = reminder.Trim();
        SaveTasks(tasks);
        return true;
    }
}
