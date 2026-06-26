# Rubric mapping

| Rubric requirement | Implemented in |
|---|---|
| Correct submission | README.md, solution, project files, assets, tasks.json, workflow |
| 6+ commits and 3 releases | Must be completed on your GitHub account |
| Task Assistant with Reminders GUI | MainWindow.xaml, MainWindow.xaml.cs, TaskManager.cs |
| JSON storage CRUD | TaskStorageHelper.cs, CyberTask.cs, tasks.json |
| Quiz with 10+ questions | QuizManager.cs, QuizQuestion.cs, Quiz tab |
| NLP simulation | ChatBot.cs, KeywordResponder.cs |
| Activity log | ActivityLogger.cs, Activity Log tab |
| Combining Parts 1, 2, 3 | MainWindow.xaml plus ChatBot.cs integrated flow |
| Video presentation | Docs/VIDEO_SCRIPT.md |

## Notes

- Tasks are stored in `tasks.json`.
- The quiz contains 15 questions.
- The activity log shows the latest 10 entries and has a Show More option.
- Part 2 improvements are retained: random responses, memory, sentiment, follow-up handling, and error handling.
