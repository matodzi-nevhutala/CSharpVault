using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BankingApp
{
    public class DashboardForm : Form
    {
        private string accountNum;
        private decimal currentBalance;
        private Label lblBalanceAmount;

        public DashboardForm(string account)
        {
            this.accountNum = account;
            this.currentBalance = Database.GetBalance(account); // Fetch live balance from SQLite

            this.Text = "VAULT BANKING // DIGITAL DASHBOARD";
            this.Size = new Size(680, 480);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(24, 24, 24);

            InitializeDashboard();
        }

        private void InitializeDashboard()
        {
            Label lblWelcome = new Label
            {
                Text = $"Welcome Back, Account #{accountNum}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(30, 25),
                AutoSize = true
            };
            this.Controls.Add(lblWelcome);

            Panel cardBalance = new Panel
            {
                Location = new Point(30, 70),
                Size = new Size(600, 100),
                BackColor = Color.FromArgb(0, 188, 212)
            };

            Label lblBalanceTitle = new Label
            {
                Text = "Available Balance",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.Black,
                Location = new Point(20, 15),
                AutoSize = true
            };
            cardBalance.Controls.Add(lblBalanceTitle);

            lblBalanceAmount = new Label
            {
                Text = $"R {currentBalance:N2}",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(18, 40),
                AutoSize = true
            };
            cardBalance.Controls.Add(lblBalanceAmount);

            this.Controls.Add(cardBalance);

            Button btnSendMoney = new Button
            {
                Text = "💸 Send Money",
                Location = new Point(30, 190),
                Size = new Size(285, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(42, 42, 42),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSendMoney.FlatAppearance.BorderColor = Color.FromArgb(0, 188, 212);
            btnSendMoney.Click += BtnSendMoney_Click;
            this.Controls.Add(btnSendMoney);

            Button btnStatement = new Button
            {
                Text = "📄 Bank Statement",
                Location = new Point(345, 190),
                Size = new Size(285, 45),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(42, 42, 42),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnStatement.FlatAppearance.BorderColor = Color.FromArgb(0, 188, 212);
            btnStatement.Click += BtnStatement_Click;
            this.Controls.Add(btnStatement);

            Label lblInfo = new Label
            {
                Text = "Quick Actions: Perform instant transfers or review recent account activity.",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(30, 250),
                AutoSize = true
            };
            this.Controls.Add(lblInfo);

            Button btnLogout = new Button
            {
                Text = "Sign Out",
                Location = new Point(30, 370),
                Size = new Size(120, 35),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.Crimson,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Click += (s, e) => this.Close();
            this.Controls.Add(btnLogout);
        }

        private void BtnSendMoney_Click(object sender, EventArgs e)
        {
            Form transferForm = new Form
            {
                Text = "TRANSFER FUNDS",
                Size = new Size(350, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            Label lblRecip = new Label { Text = "Recipient Account:", ForeColor = Color.White, Location = new Point(20, 20), AutoSize = true };
            TextBox txtRecip = new TextBox { Location = new Point(20, 45), Size = new Size(290, 25), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Label lblAmount = new Label { Text = "Amount (R):", ForeColor = Color.White, Location = new Point(20, 80), AutoSize = true };
            TextBox txtAmount = new TextBox { Location = new Point(20, 105), Size = new Size(290, 25), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            Button btnConfirm = new Button
            {
                Text = "Confirm Transfer",
                Location = new Point(20, 150),
                Size = new Size(290, 35),
                BackColor = Color.FromArgb(0, 188, 212),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btnConfirm.Click += (s, args) =>
            {
                string recipient = txtRecip.Text.Trim();
                if (string.IsNullOrEmpty(recipient))
                {
                    MessageBox.Show("Please enter a valid recipient account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (decimal.TryParse(txtAmount.Text.Trim(), out decimal amount) && amount > 0)
                {
                    if (amount > currentBalance)
                    {
                        MessageBox.Show("Insufficient funds for this transaction.", "Transfer Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Execute SQL Transaction
                    if (Database.ExecuteTransfer(accountNum, recipient, amount))
                    {
                        currentBalance = Database.GetBalance(accountNum);
                        lblBalanceAmount.Text = $"R {currentBalance:N2}";

                        MessageBox.Show($"Successfully transferred R {amount:N2} to Account #{recipient}!", "Transfer Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        transferForm.Close();
                    }
                    else
                    {
                        MessageBox.Show("Transaction failed. Please try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please enter a valid numeric amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            transferForm.Controls.AddRange(new Control[] { lblRecip, txtRecip, lblAmount, txtAmount, btnConfirm });
            transferForm.ShowDialog(this);
        }

        private void BtnStatement_Click(object sender, EventArgs e)
        {
            Form statementForm = new Form
            {
                Text = $"STATEMENT // ACCOUNT #{accountNum}",
                Size = new Size(500, 380),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(18, 18, 18)
            };

            Label lblHeader = new Label
            {
                Text = "Official Transaction Record",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 188, 212),
                Location = new Point(20, 15),
                AutoSize = true
            };

            ListBox lstTransactions = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(445, 230),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Fetch live statement entries from SQLite
            var records = Database.GetTransactions(accountNum);
            foreach (string entry in records)
            {
                lstTransactions.Items.Add(entry);
            }

            Button btnClose = new Button
            {
                Text = "Close Statement",
                Location = new Point(345, 295),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.Click += (s, args) => statementForm.Close();

            statementForm.Controls.AddRange(new Control[] { lblHeader, lstTransactions, btnClose });
            statementForm.ShowDialog(this);
        }
    }
}