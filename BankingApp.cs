using System;
using System.Drawing;
using System.Windows.Forms;

namespace BankingApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Database.Initialize(); // Initializes SQLite file and tables at startup

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }

    public class LoginForm : Form
    {
        private TextBox txtAccountNumber;
        private TextBox txtPin;
        private Button btnLogin;
        private Label lblStatus;

        public LoginForm()
        {
            this.Text = "SECURE BANK // LOGIN";
            this.Size = new Size(400, 320);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(18, 18, 18);

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            Label lblTitle = new Label
            {
                Text = "🔒 Vault Banking Login",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 188, 212),
                Location = new Point(30, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            Label lblAccount = new Label
            {
                Text = "Account Number:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(30, 75),
                AutoSize = true
            };
            this.Controls.Add(lblAccount);

            txtAccountNumber = new TextBox
            {
                Location = new Point(30, 100),
                Size = new Size(320, 25),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(42, 42, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtAccountNumber);

            Label lblPin = new Label
            {
                Text = "PIN Code:",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(30, 140),
                AutoSize = true
            };
            this.Controls.Add(lblPin);

            txtPin = new TextBox
            {
                Location = new Point(30, 165),
                Size = new Size(320, 25),
                Font = new Font("Segoe UI", 10),
                PasswordChar = '*',
                BackColor = Color.FromArgb(42, 42, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtPin);

            btnLogin = new Button
            {
                Text = "AUTHENTICATE",
                Location = new Point(30, 210),
                Size = new Size(320, 35),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 188, 212),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            this.Controls.Add(btnLogin);

            lblStatus = new Label
            {
                Location = new Point(30, 250),
                Size = new Size(320, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.LightCoral,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblStatus);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string account = txtAccountNumber.Text.Trim();
            string pin = txtPin.Text.Trim();

            // Validate against SQLite Database
            if (Database.ValidateLogin(account, pin))
            {
                this.Hide();

                DashboardForm dashboard = new DashboardForm(account);
                dashboard.FormClosed += (s, args) => this.Close();
                dashboard.Show();
            }
            else
            {
                lblStatus.ForeColor = Color.LightCoral;
                lblStatus.Text = "Invalid Account Number or PIN.";
            }
        }
    }
}