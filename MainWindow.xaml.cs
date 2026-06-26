using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CybersecurityChatbot;

public partial class MainWindow : Window
{
    private readonly ObservableCollection<CyberTask> _tasks = new();
    private readonly List<ChatMessage> _transcript = new();
    private readonly TaskStorageHelper _taskStorage;
    private readonly TaskManager _taskManager;
    private readonly ActivityLogger _activityLogger;
    private readonly QuizManager _quizManager;
    private readonly ChatBot _chatBot;
    private readonly VoiceGreetingPlayer _voiceGreetingPlayer = new();

    public MainWindow()
    {
        InitializeComponent();
        _activityLogger = new ActivityLogger();
        _taskStorage = new TaskStorageHelper();
        _taskManager = new TaskManager(_taskStorage, _activityLogger);
        _quizManager = new QuizManager(_activityLogger);
        _chatBot = new ChatBot(_taskManager, _quizManager, _activityLogger);
        TasksDataGrid.ItemsSource = _tasks;
        _activityLogger.LogChanged += ActivityLogger_LogChanged;
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AsciiArtTextBlock.Text = AsciiLogoProvider.Load();
        StatusTextBlock.Text = _voiceGreetingPlayer.Play();
        AddBotMessage(_chatBot.GetWelcomeMessage());
        RefreshTasks();
        RefreshActivityLog(recentOnly: true);
        RenderQuizQuestion();
        UpdateMemoryPanel();
        UserInputTextBox.Focus();
    }

    private void ActivityLogger_LogChanged(object? sender, EventArgs e)
    {
        RefreshActivityLog(recentOnly: true);
    }

    private void ReplayVoiceButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = _voiceGreetingPlayer.Play();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        SubmitUserMessage();
    }

    private void UserInputTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            SubmitUserMessage();
        }
    }

    private void QuickPrompt_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string prompt)
        {
            MainTabs.SelectedItem = ChatTab;
            UserInputTextBox.Text = prompt;
            SubmitUserMessage();
        }
    }

    private void SubmitUserMessage()
    {
        string input = UserInputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            AddBotMessage("Please type a message or use a quick demonstration button.");
            return;
        }

        AddUserMessage(input);
        UserInputTextBox.Clear();

        try
        {
            string response = _chatBot.ProcessInput(input);
            AddBotMessage(response);
            HandleGuiAction(_chatBot.LastGuiAction);
        }
        catch (Exception ex)
        {
            AddBotMessage($"Something went wrong, but the app is still running: {ex.Message}");
        }

        UpdateMemoryPanel();
        UserInputTextBox.Focus();
    }

    private void HandleGuiAction(string action)
    {
        switch (action)
        {
            case "RefreshTasks":
                RefreshTasks();
                MainTabs.SelectedItem = TaskTab;
                break;
            case "OpenTasks":
                RefreshTasks();
                MainTabs.SelectedItem = TaskTab;
                break;
            case "OpenQuiz":
                MainTabs.SelectedItem = QuizTab;
                RenderQuizQuestion();
                break;
            case "OpenActivityLog":
                MainTabs.SelectedItem = ActivityTab;
                RefreshActivityLog(recentOnly: true);
                break;
        }
    }

    private void AddUserMessage(string message)
    {
        AddChatBubble("You", message, new SolidColorBrush(Color.FromRgb(15, 52, 67)), Brushes.White, HorizontalAlignment.Right);
    }

    private void AddBotMessage(string message)
    {
        AddChatBubble("Chatbot", message, new SolidColorBrush(Color.FromRgb(230, 245, 244)), new SolidColorBrush(Color.FromRgb(29, 41, 57)), HorizontalAlignment.Left);
    }

    private void AddChatBubble(string speaker, string message, Brush background, Brush foreground, HorizontalAlignment alignment)
    {
        _transcript.Add(new ChatMessage(DateTime.Now, speaker, message));

        Border bubble = new()
        {
            Background = background,
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(12),
            Margin = new Thickness(0, 0, 0, 10),
            MaxWidth = 720,
            HorizontalAlignment = alignment
        };

        StackPanel content = new();
        content.Children.Add(new TextBlock
        {
            Text = speaker,
            FontWeight = FontWeights.Bold,
            Foreground = foreground,
            Margin = new Thickness(0, 0, 0, 4)
        });
        content.Children.Add(new TextBlock
        {
            Text = message,
            Foreground = foreground,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 14,
            LineHeight = 20
        });

        bubble.Child = content;
        ChatPanel.Children.Add(bubble);
        ChatScrollViewer.ScrollToEnd();
    }

    private void UpdateMemoryPanel()
    {
        MemoryTextBlock.Text = _chatBot.GetMemorySummary();
        StatusTextBlock.Text = $"Ready | Turns: {_chatBot.Memory.TurnCount} | Current topic: {_chatBot.Memory.CurrentTopic}";
    }

    private void CopyTranscriptButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string text = string.Join(Environment.NewLine, _transcript.Select(message => message.ToString()));
            Clipboard.SetText(string.IsNullOrWhiteSpace(text) ? "No messages yet." : text);
            StatusTextBlock.Text = "Transcript copied.";
            _activityLogger.Log("Chat transcript copied to clipboard.");
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = $"Could not copy transcript: {ex.Message}";
        }
    }

    private void ClearChatButton_Click(object sender, RoutedEventArgs e)
    {
        ChatPanel.Children.Clear();
        _transcript.Clear();
        AddBotMessage("The visible chat was cleared. Your saved tasks and activity log were not deleted.");
        _activityLogger.Log("Visible chat cleared.");
    }

    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CyberTask task = _taskManager.AddTask(
                TaskTitleTextBox.Text,
                TaskDescriptionTextBox.Text,
                TaskReminderTextBox.Text);

            TaskStatusTextBlock.Text = $"Task added to tasks.json: {task.Title}";
            TaskTitleTextBox.Clear();
            TaskDescriptionTextBox.Clear();
            TaskReminderTextBox.Clear();
            RefreshTasks();
        }
        catch (Exception ex)
        {
            TaskStatusTextBlock.Text = ex.Message;
        }
    }

    private void RefreshTasksButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshTasks();
    }

    private void RefreshTasks()
    {
        _tasks.Clear();
        foreach (CyberTask task in _taskManager.GetAllTasks())
        {
            _tasks.Add(task);
        }

        TaskStatusTextBlock.Text = $"Loaded {_tasks.Count} task(s) from {_taskManager.TaskFilePath}";
    }

    private CyberTask? GetSelectedTask()
    {
        return TasksDataGrid.SelectedItem as CyberTask;
    }

    private void MarkCompleteButton_Click(object sender, RoutedEventArgs e)
    {
        CyberTask? task = GetSelectedTask();
        if (task is null)
        {
            TaskStatusTextBlock.Text = "Select a task first.";
            return;
        }

        _taskManager.MarkAsComplete(task.Id);
        TaskStatusTextBlock.Text = $"Marked complete: {task.Title}";
        RefreshTasks();
    }

    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        CyberTask? task = GetSelectedTask();
        if (task is null)
        {
            TaskStatusTextBlock.Text = "Select a task first.";
            return;
        }

        MessageBoxResult confirm = MessageBox.Show(
            $"Delete task '{task.Title}'?",
            "Confirm delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            _taskManager.DeleteTask(task.Id);
            TaskStatusTextBlock.Text = $"Deleted: {task.Title}";
            RefreshTasks();
        }
    }

    private void StartQuizButton_Click(object sender, RoutedEventArgs e)
    {
        _quizManager.ResetQuiz();
        RenderQuizQuestion();
        QuizFeedbackTextBlock.Text = "Quiz started. Choose one answer and click Submit Answer.";
    }

    private void SubmitQuizAnswerButton_Click(object sender, RoutedEventArgs e)
    {
        string? answer = GetSelectedQuizAnswer();
        if (string.IsNullOrWhiteSpace(answer))
        {
            QuizFeedbackTextBlock.Text = "Please select an answer first.";
            return;
        }

        QuizAnswerResult result = _quizManager.SubmitAnswer(answer);
        QuizFeedbackTextBlock.Text = result.Feedback + Environment.NewLine + result.Explanation;
        QuizScoreTextBlock.Text = $"Score: {result.Score} / {result.Total}";
    }

    private void NextQuestionButton_Click(object sender, RoutedEventArgs e)
    {
        bool hasNext = _quizManager.MoveNext();
        if (!hasNext && _quizManager.IsFinished())
        {
            QuizQuestionTextBlock.Text = _quizManager.GetFinalScore();
            QuizOptionsPanel.Children.Clear();
            QuizFeedbackTextBlock.Text = _quizManager.GetFinalMessage();
            QuizScoreTextBlock.Text = $"Score: {_quizManager.Score} / {_quizManager.TotalQuestions}";
            return;
        }

        RenderQuizQuestion();
    }

    private void RenderQuizQuestion()
    {
        QuizOptionsPanel.Children.Clear();
        QuizQuestion? question = _quizManager.GetCurrentQuestion();

        if (question is null)
        {
            QuizQuestionTextBlock.Text = "Click Start / Restart Quiz to begin.";
            QuizScoreTextBlock.Text = $"Score: {_quizManager.Score} / {_quizManager.TotalQuestions}";
            return;
        }

        QuizQuestionTextBlock.Text = $"Question {_quizManager.CurrentNumber} of {_quizManager.TotalQuestions}: {question.Question}";
        QuizScoreTextBlock.Text = $"Score: {_quizManager.Score} / {_quizManager.TotalQuestions}";
        QuizFeedbackTextBlock.Text = string.Empty;

        foreach (string option in question.Options)
        {
            RadioButton radio = new()
            {
                Content = option,
                GroupName = "QuizOptions",
                FontSize = 16,
                Margin = new Thickness(0, 6, 0, 6),
                Tag = ExtractAnswerValue(option)
            };
            QuizOptionsPanel.Children.Add(radio);
        }
    }

    private string? GetSelectedQuizAnswer()
    {
        foreach (object child in QuizOptionsPanel.Children)
        {
            if (child is RadioButton radio && radio.IsChecked == true)
            {
                return radio.Tag?.ToString();
            }
        }

        return null;
    }

    private static string ExtractAnswerValue(string option)
    {
        string trimmed = option.Trim();
        if (trimmed.StartsWith("A)", StringComparison.OrdinalIgnoreCase)) return "A";
        if (trimmed.StartsWith("B)", StringComparison.OrdinalIgnoreCase)) return "B";
        if (trimmed.StartsWith("C)", StringComparison.OrdinalIgnoreCase)) return "C";
        if (trimmed.StartsWith("D)", StringComparison.OrdinalIgnoreCase)) return "D";
        return trimmed;
    }

    private void ShowRecentLogButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshActivityLog(recentOnly: true);
    }

    private void ShowMoreLogButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshActivityLog(recentOnly: false);
    }

    private void RefreshActivityButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshActivityLog(recentOnly: true);
    }

    private void RefreshActivityLog(bool recentOnly)
    {
        ActivityLogTextBlock.Text = recentOnly
            ? _activityLogger.GetRecentLog(10)
            : _activityLogger.GetFullLog();
    }
}
