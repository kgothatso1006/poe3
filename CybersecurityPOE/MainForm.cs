using System;
using System.Collections.Generic;
using System.Drawing;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace CybersecurityPOE
{
    public partial class MainForm : Form
    {
      // SQL Server Connection String (LocalDB)
     private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=CybersecurityPOE;Integrated Security=True;";

        private SpeechSynthesizer speechSynthesizer;
        private List<ActivityLog> activityLog;
        private List<QuizQuestion> questions;
        private int currentQ = 0;
        private int score = 0;
        private bool quizActive = false;

        // UI Controls
        private TextBox txtInput;
        private RichTextBox txtChat;
        private ListBox lstTasks;
        private Label lblStatus;
        private Button btnSend;
        private Button btnRefresh;
        private Button btnDelete;
        private Button btnComplete;
        private Button btnLog;
        private Button btnQuiz;
        private Button btnSubmit;
        private Label lblQuestion;
        private Label lblScore;
        private RadioButton[] radioOptions;

        public MainForm()
        {
            try
            {
                speechSynthesizer = new SpeechSynthesizer();
                speechSynthesizer.Volume = 100;
                speechSynthesizer.Rate = 1;
            }
            catch { }

            InitializeComponent();
            SetupForm();
            EnsureDatabaseSetup();
            LoadTasks();
            LogActivity("System", "App Started");
            AddMessage("Bot: Welcome to Cybersecurity POE. Type 'help' for commands.");
        }

        private void SetupForm()
        {
            this.Text = "Cybersecurity POE with Voice";
            this.Size = new Size(1100, 700);
            this.BackColor = Color.FromArgb(30, 30, 50);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void InitializeComponent()
        {
            // Chat Display
            txtChat = new RichTextBox
            {
                Location = new Point(20, 20),
                Size = new Size(500, 400),
                BackColor = Color.Black,
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ReadOnly = true
            };

            // Chat Input
            txtInput = new TextBox
            {
                Location = new Point(20, 430),
                Size = new Size(400, 30),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            btnSend = new Button
            {
                Text = "Send",
                Location = new Point(430, 428),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSend.Click += BtnSend_Click;

            // Tasks List
            lstTasks = new ListBox
            {
                Location = new Point(540, 20),
                Size = new Size(520, 180),
                BackColor = Color.White,
                Font = new Font("Consolas", 10)
            };

            // Button: Refresh Tasks
            btnRefresh = new Button
            {
                Text = "Refresh Tasks",
                Location = new Point(540, 210),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.Click += (s, e) => LoadTasks();

            // Button: Delete Task
            btnDelete = new Button
            {
                Text = "Delete Task",
                Location = new Point(670, 210),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnDelete.Click += BtnDeleteTask_Click;

            // Button: Activity Log
            btnLog = new Button
            {
                Text = "Activity Log",
                Location = new Point(780, 210),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLog.Click += (s, e) => ShowLog();

            // Button: Mark Complete
            btnComplete = new Button
            {
                Text = "Mark Complete",
                Location = new Point(540, 245),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 100, 200),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnComplete.Click += BtnCompleteTask_Click;

            // Button: Add Sample Task
            Button btnAddSample = new Button
            {
                Text = "Add Sample Task",
                Location = new Point(670, 245),
                Size = new Size(210, 30),
                BackColor = Color.FromArgb(0, 150, 150),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnAddSample.Click += (s, e) => AddSampleTask();

            // Quiz Area
            lblQuestion = new Label
            {
                Location = new Point(540, 290),
                Size = new Size(520, 40),
                Text = "Click 'Start Quiz' to begin!",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            radioOptions = new RadioButton[4];
            for (int i = 0; i < 4; i++)
            {
                radioOptions[i] = new RadioButton
                {
                    Location = new Point(540, 340 + (i * 25)),
                    Size = new Size(500, 20),
                    ForeColor = Color.White,
                    Visible = false,
                    Font = new Font("Segoe UI", 9)
                };
            }

            btnSubmit = new Button
            {
                Text = "Submit Answer",
                Location = new Point(540, 450),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(128, 0, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            btnSubmit.Click += BtnSubmit_Click;

            lblScore = new Label
            {
                Text = "Score: 0",
                Location = new Point(680, 450),
                Size = new Size(150, 30),
                ForeColor = Color.Yellow,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnQuiz = new Button
            {
                Text = "Start Quiz",
                Location = new Point(540, 500),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(200, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnQuiz.Click += (s, e) => StartQuiz();

            // Status Label
            lblStatus = new Label
            {
                Text = "Ready",
                Location = new Point(20, 480),
                Size = new Size(500, 25),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9)
            };

            // Add all controls to the form
            this.Controls.Add(txtChat);
            this.Controls.Add(txtInput);
            this.Controls.Add(btnSend);
            this.Controls.Add(lstTasks);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(btnDelete);
            this.Controls.Add(btnLog);
            this.Controls.Add(btnComplete);
            this.Controls.Add(btnAddSample);
            this.Controls.Add(lblQuestion);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(lblScore);
            this.Controls.Add(btnQuiz);
            this.Controls.Add(lblStatus);
            foreach (var rb in radioOptions) this.Controls.Add(rb);
        }

        private void EnsureDatabaseSetup()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        IF OBJECT_ID('dbo.Tasks', 'U') IS NULL
                        BEGIN
                            CREATE TABLE Tasks (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Title VARCHAR(200) NOT NULL,
                                Description TEXT,
                                ReminderDate DATETIME,
                                IsCompleted BIT DEFAULT 0,
                                CreatedAt DATETIME DEFAULT GETDATE()
                            );

                            INSERT INTO Tasks (Title, Description, ReminderDate) VALUES 
                            ('Enable Two-Factor Authentication', 'Add 2FA to your email and banking accounts', DATEADD(DAY, 7, GETDATE())),
                            ('Review Privacy Settings', 'Check social media privacy settings', DATEADD(DAY, 3, GETDATE())),
                            ('Update Passwords', 'Change passwords for critical accounts', DATEADD(DAY, 14, GETDATE()));
                        END
                    ";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    LogActivity("System", "Database checked/created");
                }
            }
            catch (Exception ex)
            {
                AddMessage("Bot: Database not found. Please create 'CybersecurityPOE' database.");
                LogActivity("Error", "DB setup failed: " + ex.Message);
            }
        }

        private void AddMessage(string msg)
        {
            txtChat.AppendText(msg + "\n");
            txtChat.ScrollToCaret();

            if (msg.StartsWith("Bot:") && speechSynthesizer != null)
            {
                string text = msg.Replace("Bot:", "").Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    try { speechSynthesizer.SpeakAsync(text); } catch { }
                }
            }
        }

        private void LogActivity(string type, string desc)
        {
            if (activityLog == null) activityLog = new List<ActivityLog>();
            activityLog.Insert(0, new ActivityLog
            {
                Timestamp = DateTime.Now,
                Type = type,
                Description = desc
            });
            if (activityLog.Count > 50) activityLog.RemoveRange(50, activityLog.Count - 50);
        }

        private void ShowLog()
        {
            if (activityLog == null || activityLog.Count == 0)
            {
                AddMessage("Bot: No activities logged yet.");
                return;
            }

            string log = "\n--- LAST 8 ACTIVITIES ---\n";
            for (int i = 0; i < Math.Min(8, activityLog.Count); i++)
            {
                log += $"{i + 1}. [{activityLog[i].Timestamp:HH:mm:ss}] {activityLog[i].Type}: {activityLog[i].Description}\n";
            }
            AddMessage("Bot: " + log);
            LogActivity("Command", "Viewed activity log");
        }

        private void LoadTasks()
        {
            lstTasks.Items.Clear();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT Id, Title, IsCompleted FROM Tasks ORDER BY IsCompleted, Id DESC";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string status = reader.GetBoolean(2) ? "[X]" : "[ ]";
                            lstTasks.Items.Add($"{status} [{reader.GetInt32(0)}] {reader.GetString(1)}");
                        }
                    }
                }
                lblStatus.Text = $"Loaded {lstTasks.Items.Count} tasks";
                LogActivity("Database", $"Loaded {lstTasks.Items.Count} tasks");
            }
            catch (Exception ex)
            {
                lstTasks.Items.Add("DATABASE NOT CONNECTED");
                lstTasks.Items.Add("Error: " + ex.Message);
                lblStatus.Text = "Database error";
            }
        }

        private void AddSampleTask()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO Tasks (Title, Description, ReminderDate) VALUES ('Backup Important Files', 'Create backups of all important documents', DATEADD(DAY, 5, GETDATE()))";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                AddMessage("Bot: Sample task added.");
                LogActivity("Task", "Added sample task");
                LoadTasks();
            }
            catch (Exception ex)
            {
                AddMessage($"Bot: Error adding task: {ex.Message}");
            }
        }

        private void BtnDeleteTask_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem == null)
            {
                AddMessage("Bot: Please select a task to delete.");
                return;
            }

            string selected = lstTasks.SelectedItem.ToString();
            var match = System.Text.RegularExpressions.Regex.Match(selected, @"\[(\d+)\]");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int taskId))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM Tasks WHERE Id = @id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", taskId);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                AddMessage($"Bot: Task #{taskId} deleted successfully.");
                                LogActivity("Task", $"Deleted task #{taskId}");
                                LoadTasks();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddMessage($"Bot: Error deleting task: {ex.Message}");
                }
            }
        }

        private void BtnCompleteTask_Click(object sender, EventArgs e)
        {
            if (lstTasks.SelectedItem == null)
            {
                AddMessage("Bot: Please select a task to complete.");
                return;
            }

            string selected = lstTasks.SelectedItem.ToString();
            var match = System.Text.RegularExpressions.Regex.Match(selected, @"\[(\d+)\]");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int taskId))
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE Tasks SET IsCompleted = 1 WHERE Id = @id";
                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", taskId);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                AddMessage($"Bot: Task #{taskId} marked as complete!");
                                LogActivity("Task", $"Completed task #{taskId}");
                                LoadTasks();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    AddMessage($"Bot: Error completing task: {ex.Message}");
                }
            }
        }

        private void StartQuiz()
        {
            if (questions == null) LoadQuestions();
            quizActive = true;
            currentQ = 0;
            score = 0;
            lblScore.Text = "Score: 0";
            ShowQuestion();
            LogActivity("Quiz", "Started quiz");
            AddMessage("Bot: Quiz started. Good luck!");
        }

        private void LoadQuestions()
        {
            questions = new List<QuizQuestion>();
            questions.Add(new QuizQuestion { Text = "What is phishing?", Options = new[] { "A type of fishing", "A cyber attack to steal information", "An antivirus", "A password manager" }, Correct = 1, Explanation = "Phishing tricks you into revealing sensitive information." });
            questions.Add(new QuizQuestion { Text = "Which is a strong password?", Options = new[] { "password123", "qwerty", "P@ssw0rd!2024", "admin" }, Correct = 2, Explanation = "Use uppercase, lowercase, numbers, and symbols." });
            questions.Add(new QuizQuestion { Text = "True or False: 2FA adds extra security", Options = new[] { "True", "False" }, Correct = 0, Explanation = "2FA requires a second verification method like a code from your phone." });
            questions.Add(new QuizQuestion { Text = "What to do with suspicious email asking for password?", Options = new[] { "Reply with password", "Forward to friends", "Report as phishing and delete", "Click all links" }, Correct = 2, Explanation = "Never share your password. Report phishing and delete immediately." });
            questions.Add(new QuizQuestion { Text = "What is ransomware?", Options = new[] { "Donation software", "Malware that encrypts files and demands payment", "Antivirus", "Backup tool" }, Correct = 1, Explanation = "Ransomware locks your files and demands payment to unlock them." });
            questions.Add(new QuizQuestion { Text = "True or False: Using same password for multiple accounts is safe", Options = new[] { "True", "False" }, Correct = 1, Explanation = "False. If one account is hacked, all your accounts become vulnerable." });
            questions.Add(new QuizQuestion { Text = "What does HTTPS stand for?", Options = new[] { "HyperText Transfer Protocol Secure", "High Tech Transfer Protocol", "Hyper Transfer Text Secure", "None" }, Correct = 0, Explanation = "HTTPS encrypts data between your browser and websites." });
            questions.Add(new QuizQuestion { Text = "What is a common sign of a phishing email?", Options = new[] { "Professional design", "Your full name", "Urgent requests and spelling errors", "No attachments" }, Correct = 2, Explanation = "Phishing emails often create urgency and contain spelling mistakes." });
            questions.Add(new QuizQuestion { Text = "What is social engineering?", Options = new[] { "Engineering social media", "Manipulating people to reveal information", "Building social networks", "Type of firewall" }, Correct = 1, Explanation = "Social engineering exploits human psychology to gain access." });
            questions.Add(new QuizQuestion { Text = "How often should you update software?", Options = new[] { "Never", "Once a year", "Regularly when updates available", "Only when crashed" }, Correct = 2, Explanation = "Updates include critical security patches." });
            questions.Add(new QuizQuestion { Text = "True or False: Public Wi-Fi is safe for banking", Options = new[] { "True", "False" }, Correct = 1, Explanation = "False. Public Wi-Fi is often unencrypted." });
            questions.Add(new QuizQuestion { Text = "What is a VPN used for?", Options = new[] { "Speed up internet", "Hide IP and encrypt connection", "Download games", "Clean computer" }, Correct = 1, Explanation = "A VPN encrypts your connection and hides your IP address." });
        }

        private void ShowQuestion()
        {
            if (currentQ >= questions.Count)
            {
                EndQuiz();
                return;
            }

            var q = questions[currentQ];
            lblQuestion.Text = $"Q{currentQ + 1}/{questions.Count}: {q.Text}";
            for (int i = 0; i < 4; i++)
            {
                if (i < q.Options.Length)
                {
                    radioOptions[i].Text = $"{(char)('A' + i)}) {q.Options[i]}";
                    radioOptions[i].Visible = true;
                    radioOptions[i].Checked = false;
                }
                else
                {
                    radioOptions[i].Visible = false;
                }
            }
            btnSubmit.Visible = true;
            btnQuiz.Visible = false;
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (!quizActive) return;

            int selected = -1;
            for (int i = 0; i < 4; i++)
            {
                if (radioOptions[i].Visible && radioOptions[i].Checked)
                {
                    selected = i;
                    break;
                }
            }

            if (selected == -1)
            {
                AddMessage("Bot: Please select an answer.");
                return;
            }

            var q = questions[currentQ];
            if (selected == q.Correct)
            {
                score++;
                lblScore.Text = $"Score: {score}";
                AddMessage($"Bot: CORRECT. {q.Explanation}");
                LogActivity("Quiz", $"Correct: {q.Text}");
            }
            else
            {
                AddMessage($"Bot: INCORRECT. The correct answer is: {q.Options[q.Correct]}\n{q.Explanation}");
                LogActivity("Quiz", $"Incorrect: {q.Text}");
            }

            currentQ++;
            ShowQuestion();
        }

        private void EndQuiz()
        {
            quizActive = false;
            btnSubmit.Visible = false;
            btnQuiz.Visible = true;
            foreach (var rb in radioOptions) rb.Visible = false;

            string feedback = score >= 9 ? "EXCELLENT. You are a cybersecurity expert." :
                              score >= 7 ? "GOOD JOB. Keep learning to stay safe." :
                              "GOOD TRY. Review the explanations and try again.";

            AddMessage($"Bot: QUIZ OVER.\nFinal Score: {score}/{questions.Count}\n{feedback}");
            LogActivity("Quiz", $"Final score: {score}/{questions.Count}");
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string input = txtInput.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(input)) return;

            AddMessage($"You: {input}");
            LogActivity("Chat", $"User: {input}");

            // NLP Keyword Detection
            if (input == "help")
            {
                AddMessage("Bot: Here are my commands:\n- 'start quiz' - Take cybersecurity quiz (12 questions)\n- 'show log' - View activity log\n- 'tasks' - Refresh task list\n- 'help' - Show this menu\n- 'what is phishing' - Ask security questions\n- 'tell me about passwords' - Get password tips");
            }
            else if (input.Contains("start quiz") || input.Contains("take quiz") || input.Contains("begin quiz"))
            {
                StartQuiz();
            }
            else if (input.Contains("show log") || input.Contains("activity log") || input.Contains("what have you done"))
            {
                ShowLog();
            }
            else if (input.Contains("tasks") || input.Contains("show tasks") || input.Contains("my tasks"))
            {
                LoadTasks();
                AddMessage("Bot: Tasks refreshed. Check the list on the right.");
            }
            else if (input.Contains("phishing") || input.Contains("what is phishing"))
            {
                AddMessage("Bot: Phishing is when attackers send fake emails or messages pretending to be legitimate companies. They try to steal your passwords and personal information. Never click suspicious links.");
            }
            else if (input.Contains("password") || input.Contains("strong password"))
            {
                AddMessage("Bot: Use strong passwords with 12+ characters, mixing uppercase, lowercase, numbers, and symbols. Never reuse passwords across different accounts. Consider using a password manager.");
            }
            else if (input.Contains("2fa") || input.Contains("two factor") || input.Contains("multi factor"))
            {
                AddMessage("Bot: Two-Factor Authentication (2FA) adds an extra security layer. Even if someone steals your password, they cannot access your account without the second factor, like a code from your phone.");
            }
            else if (input.Contains("ransomware"))
            {
                AddMessage("Bot: Ransomware is malware that encrypts your files and demands payment. Always backup your important files to protect against ransomware attacks.");
            }
            else if (input.Contains("vpn"))
            {
                AddMessage("Bot: A VPN (Virtual Private Network) encrypts your internet connection and hides your IP address. It is especially important when using public Wi-Fi.");
            }
            else if (input == "hello" || input == "hi" || input == "hey")
            {
                AddMessage("Bot: Hello. I am your cybersecurity assistant. Type 'help' to see what I can do.");
            }
            else if (input == "bye" || input == "exit" || input == "quit")
            {
                AddMessage("Bot: Goodbye. Stay safe online.");
                LogActivity("System", "User exited");
            }
            else
            {
                AddMessage("Bot: I am your cybersecurity assistant. Try typing 'help' to see my commands, or ask me about phishing, passwords, 2FA, or ransomware.");
            }

            txtInput.Clear();
        }
    }

    // Activity Log Class
    public class ActivityLog
    {
        public DateTime Timestamp { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    // Quiz Question Class
    public class QuizQuestion
    {
        public string Text { get; set; }
        public string[] Options { get; set; }
        public int Correct { get; set; }
        public string Explanation { get; set; }
    }
}