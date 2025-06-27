// ============================
// POE_PROG_PART3 FULL PROJECT
// ============================
// - GUI Task Assistant with Reminders
// - Cybersecurity Quiz Mini-Game
// - NLP Keyword Detection & Sentiment
// ============================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace POE_PROG_PART3
{
    public partial class ChatbotMainForm : Form
    {
        private List<TaskItem> taskList = new List<TaskItem>();
        private int quizScore = 0;
        private int quizIndex = 0;
        private readonly List<QuizQuestion> questions = new List<QuizQuestion>();
        private SentimentAnalyzer sentimentAnalyzer = new SentimentAnalyzer();

        public ChatbotMainForm()
        {
            InitializeComponent();
            LoadGreeting();
            LoadQuestions();
        }

        private void LoadGreeting()
        {
            try
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\Debug\\", "");
                string audioPath = Path.Combine(basePath, "greeting.wav");
                if (File.Exists(audioPath))
                {
                    SoundPlayer player = new SoundPlayer(audioPath);
                    player.PlaySync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greeting error: " + ex.Message);
            }
        }

        private void LoadQuestions()
        {
            questions.Add(new QuizQuestion("What should you do if you receive an email asking for your password?", new[] { "A) Reply with your password", "B) Delete the email", "C) Report the email as phishing", "D) Ignore it" }, "C"));
            questions.Add(new QuizQuestion("True or False: Strong passwords should include letters, numbers, and symbols.", new[] { "A) True", "B) False" }, "A"));
            questions.Add(new QuizQuestion("Which of these is a good cybersecurity practice?", new[] { "A) Sharing passwords", "B) Updating software regularly", "C) Using the same password", "D) Clicking unknown links" }, "B"));
            questions.Add(new QuizQuestion("What does 2FA stand for?", new[] { "A) Two-Factor Authentication", "B) Two-Faced Application", "C) File Access", "D) None" }, "A"));
            questions.Add(new QuizQuestion("True or False: You should use public Wi-Fi for banking.", new[] { "A) True", "B) False" }, "B"));
            questions.Add(new QuizQuestion("What is phishing?", new[] { "A) A virus", "B) A type of scam", "C) A firewall", "D) A hacker name" }, "B"));
            questions.Add(new QuizQuestion("Which one is a safe website URL?", new[] { "A) http://", "B) ftp://", "C) https://", "D) none" }, "C"));
            questions.Add(new QuizQuestion("What is malware?", new[] { "A) Clothing", "B) Software to harm systems", "C) Music app", "D) Safe website" }, "B"));
            questions.Add(new QuizQuestion("How often should you change your password?", new[] { "A) Never", "B) Once a year", "C) Regularly", "D) Every day" }, "C"));
            questions.Add(new QuizQuestion("What’s the safest way to store passwords?", new[] { "A) On paper", "B) Password manager", "C) In email", "D) Memory only" }, "B"));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text.ToLower();
            lstChat.Items.Add("You: " + input);

            string detectedSentiment = sentimentAnalyzer.DetectSentiment(input);
            string reply = sentimentAnalyzer.GetResponseForSentiment(detectedSentiment);

            if (input.Contains("task") || input.Contains("remind"))
            {
                AddTaskPrompt();
            }
            else if (input.Contains("quiz") || input.Contains("test"))
            {
                StartQuiz();
            }
            else if (input.Contains("show tasks"))
            {
                ShowTasks();
            }
            else if (input.Contains("log"))
            {
                lstChat.Items.Add("Bot: Tasks completed: " + taskList.Count(t => t.Completed) + ", Quiz score: " + quizScore);
            }
            else
            {
                lstChat.Items.Add("Bot: " + reply);
            }

            txtInput.Clear();
        }

        private void AddTaskPrompt()
        {
            string title = Microsoft.VisualBasic.Interaction.InputBox("Enter task title:", "Add Task");
            string desc = Microsoft.VisualBasic.Interaction.InputBox("Enter task description:", "Add Task");
            string reminder = Microsoft.VisualBasic.Interaction.InputBox("Enter reminder date (yyyy-mm-dd) or leave blank:", "Add Task");

            DateTime? remindDate = null;
            if (DateTime.TryParse(reminder, out DateTime parsedDate))
            {
                remindDate = parsedDate;
            }

            taskList.Add(new TaskItem { Title = title, Description = desc, ReminderDate = remindDate });
            lstChat.Items.Add("Bot: Task added successfully!");
        }

        private void ShowTasks()
        {
            foreach (var task in taskList)
            {
                lstChat.Items.Add("- " + task.Title + ": " + task.Description + (task.ReminderDate.HasValue ? " | Reminder: " + task.ReminderDate.Value.ToShortDateString() : ""));
            }
        }

        private void StartQuiz()
        {
            quizIndex = 0;
            quizScore = 0;
            ShowNextQuizQuestion();
        }

        private void ShowNextQuizQuestion()
        {
            if (quizIndex >= questions.Count)
            {
                lstChat.Items.Add("Bot: Quiz finished! Your score: " + quizScore + "/10");
                lstChat.Items.Add(quizScore >= 7 ? "Bot: Great job! You're a cybersecurity pro!" : "Bot: Keep learning to stay safe online!");
                return;
            }

            QuizQuestion q = questions[quizIndex];
            lstChat.Items.Add("Bot: " + q.Question);
            foreach (string opt in q.Options)
                lstChat.Items.Add(opt);

            string answer = Microsoft.VisualBasic.Interaction.InputBox("Enter answer (e.g. A/B/C/D):", "Quiz").ToUpper();
            if (answer == q.Answer)
            {
                lstChat.Items.Add("Correct! " + q.Feedback);
                quizScore++;
            }
            else
            {
                lstChat.Items.Add("Incorrect. " + q.Feedback);
            }

            quizIndex++;
            ShowNextQuizQuestion();
        }
    }

    public class TaskItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? ReminderDate { get; set; }
        public bool Completed { get; set; } = false;
    }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public string[] Options { get; set; }
        public string Answer { get; set; }
        public string Feedback { get; set; }

        public QuizQuestion(string q, string[] o, string a)
        {
            Question = q;
            Options = o;
            Answer = a;
            Feedback = GenerateFeedback(a);
        }

        private string GenerateFeedback(string a)
        {
            switch (a)
            {
                case "C": return "Reporting phishing emails helps prevent scams.";
                case "A": return "Correct! That's a best practice.";
                case "B": return "You're right! Well spotted.";
                default: return "Keep that in mind for the future!";
            }
        }
    }

    public class SentimentAnalyzer
    {
        private readonly Dictionary<string, string[]> sentimentKeywords = new Dictionary<string, string[]>
        {
            { "worried", new[] { "worried", "concerned", "anxious", "nervous", "scared" } },
            { "frustrated", new[] { "frustrated", "angry", "annoyed", "irritated", "mad" } },
            { "curious", new[] { "curious", "wondering", "interested", "intrigued" } },
            { "overwhelmed", new[] { "overwhelmed", "confused", "lost", "unsure" } }
        };

        private readonly Dictionary<string, string[]> sentimentResponses = new Dictionary<string, string[]>
        {
            { "worried", new[] { "It's okay to feel that way. I'm here to help.", "Let me provide some clarity for you." } },
            { "frustrated", new[] { "I hear your frustration. Let's work through it.", "I'll explain things clearly." } },
            { "curious", new[] { "Great question! Here's some info.", "I love your curiosity!" } },
            { "overwhelmed", new[] { "Let's take it step by step.", "I know it seems like a lot, but we can do it together." } }
        };

        public string DetectSentiment(string input)
        {
            foreach (var pair in sentimentKeywords)
            {
                foreach (string word in pair.Value)
                {
                    if (input.Contains(word)) return pair.Key;
                }
            }
            return null;
        }

        public string GetResponseForSentiment(string sentiment)
        {
            if (sentiment != null && sentimentResponses.ContainsKey(sentiment))
            {
                var rand = new Random();
                var responses = sentimentResponses[sentiment];
                return responses[rand.Next(responses.Length)];
            }
            return "I'm here to help you with cybersecurity or reminders. Just ask!";
        }
    }
}