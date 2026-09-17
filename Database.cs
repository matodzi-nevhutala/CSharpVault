using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace BankingApp
{
    public static class Database
    {
        private const string ConnectionString = "Data Source=bank.db";

        public static void Initialize()
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();

                string createUsersTable = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        AccountNumber TEXT PRIMARY KEY,
                        Pin TEXT NOT NULL,
                        Balance DECIMAL NOT NULL
                    );";

                string createTxTable = @"
                    CREATE TABLE IF NOT EXISTS Transactions (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountNumber TEXT NOT NULL,
                        Description TEXT NOT NULL,
                        Amount DECIMAL NOT NULL,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                using (var cmd = new SqliteCommand(createUsersTable, connection)) { cmd.ExecuteNonQuery(); }
                using (var cmd = new SqliteCommand(createTxTable, connection)) { cmd.ExecuteNonQuery(); }

                string seedUser = @"
                    INSERT OR IGNORE INTO Users (AccountNumber, Pin, Balance)
                    VALUES ('1001', '1234', 12450.80);";
                
                using (var cmd = new SqliteCommand(seedUser, connection)) { cmd.ExecuteNonQuery(); }
            }
        }

        public static bool ValidateLogin(string account, string pin)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE AccountNumber = @acc AND Pin = @pin;";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@acc", account);
                    cmd.Parameters.AddWithValue("@pin", pin);
                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public static decimal GetBalance(string account)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Balance FROM Users WHERE AccountNumber = @acc;";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@acc", account);
                    object result = cmd.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0.00m;
                }
            }
        }

        public static bool ExecuteTransfer(string senderAcc, string recipAcc, decimal amount)
        {
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string deductQuery = "UPDATE Users SET Balance = Balance - @amt WHERE AccountNumber = @acc;";
                        using (var cmd = new SqliteCommand(deductQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@amt", amount);
                            cmd.Parameters.AddWithValue("@acc", senderAcc);
                            cmd.ExecuteNonQuery();
                        }

                        string logTxQuery = "INSERT INTO Transactions (AccountNumber, Description, Amount) VALUES (@acc, @desc, @amt);";
                        using (var cmd = new SqliteCommand(logTxQuery, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@acc", senderAcc);
                            cmd.Parameters.AddWithValue("@desc", $"Transfer to #{recipAcc}");
                            cmd.Parameters.AddWithValue("@amt", -amount);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static List<string> GetTransactions(string account)
        {
            var history = new List<string>();
            using (var connection = new SqliteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT Timestamp, Description, Amount FROM Transactions WHERE AccountNumber = @acc ORDER BY Id DESC;";
                using (var cmd = new SqliteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@acc", account);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string time = reader.GetDateTime(0).ToString("yyyy-MM-dd HH:mm");
                            string desc = reader.GetString(1);
                            decimal amt = reader.GetDecimal(2);
                            string sign = amt >= 0 ? "+" : "";
                            history.Add($"{time} | {desc} | {sign}R {Math.Abs(amt):N2}");
                        }
                    }
                }
            }
            return history;
        }
    }
}