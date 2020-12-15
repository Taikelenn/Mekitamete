using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Mekitamete.Database
{
    public partial class DBConnection : IDisposable
    {
        public void InitializeDatabase()
        {
            using (SQLiteCommand query = new SQLiteCommand("CREATE TABLE IF NOT EXISTS transactions (" +
                "id INTEGER NOT NULL," +
                "status INTEGER NOT NULL DEFAULT 0," +
                "currency INTEGER NOT NULL," +
                "value INTEGER NOT NULL," +
                "minConfirmations INTEGER NOT NULL DEFAULT 1," +
                "note TEXT," +
                "successUrl TEXT," +
                "failureUrl TEXT," +
                "PRIMARY KEY (id)" +
                ")", dbConnection))
            {
                query.ExecuteNonQuery();
            }


            using (SQLiteCommand query = new SQLiteCommand("CREATE TABLE IF NOT EXISTS addresses (" +
                "transactionId INTEGER NOT NULL," +
                "address TEXT NOT NULL," +
                "PRIMARY KEY (transactionId, address)" +
                ")", dbConnection))
            {
                query.ExecuteNonQuery();
            }
        }

        public bool IsIDUsed(ulong id)
        {
            using (SQLiteCommand query = new SQLiteCommand("SELECT 1 FROM transactions WHERE id = @transId", dbConnection))
            {
                query.Parameters.AddWithValue("@transId", id);

                return query.ExecuteScalar() != null;
            }
        }

        private void InsertNewTransaction(Transaction transaction)
        {
            using (SQLiteCommand insertQuery = new SQLiteCommand("INSERT INTO transactions (id, status, currency, value, minConfirmations, note, successUrl, failureUrl)" +
                "VALUES (@newId, @status, @currency, @value, @minConf, @note, @successUrl, @failureUrl)", dbConnection))
            {
                insertQuery.Parameters.AddWithValue("@newId", transaction.Id);
                insertQuery.Parameters.AddWithValue("@status", transaction.Status);
                insertQuery.Parameters.AddWithValue("@currency", transaction.Currency);
                insertQuery.Parameters.AddWithValue("@value", transaction.PaymentAmount);
                insertQuery.Parameters.AddWithValue("@minConf", transaction.MinConfirmations);
                insertQuery.Parameters.AddWithValue("@note", transaction.Note);
                insertQuery.Parameters.AddWithValue("@successUrl", transaction.SuccessUrl);
                insertQuery.Parameters.AddWithValue("@failureUrl", transaction.FailureUrl);
                insertQuery.ExecuteNonQuery();
            }
        }

        public void AddNewAddress(Transaction transaction, string address)
        {
            using (SQLiteCommand insertQuery = new SQLiteCommand("INSERT INTO addresses (transactionId, address) VALUES (@transId, @addr)", dbConnection))
            {
                insertQuery.Parameters.AddWithValue("@transId", transaction.Id);
                insertQuery.Parameters.AddWithValue("@addr", address);
                insertQuery.ExecuteNonQuery();
            }
        }

        public void CreateNewTransaction(Transaction transaction)
        {
            using (SQLiteTransaction t = dbConnection.BeginTransaction())
            {
                InsertNewTransaction(transaction);
                transaction.AddNewAddress();

                t.Commit();
            }
        }

        public Transaction GetTransaction(ulong transactionId)
        {
            using (SQLiteCommand transQuery = new SQLiteCommand("SELECT id, status, currency, value, minConfirmations, note, successUrl, failureUrl FROM transactions " +
                "WHERE id = @transId", dbConnection))
            {
                transQuery.Parameters.AddWithValue("@transId", transactionId);

                using (SQLiteDataReader reader = transQuery.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return Transaction.CreateFromDatabaseEntry(reader);
                    }
                }
            }

            return null;
        }

        public void UpdateTransactionStatus(Transaction transaction)
        {
            using (SQLiteCommand statusQuery = new SQLiteCommand("UPDATE transactions SET status = @newStatus WHERE id = @transId", dbConnection))
            {
                statusQuery.Parameters.AddWithValue("@transId", transaction.Id);
                statusQuery.Parameters.AddWithValue("@newStatus", transaction.Status);
                statusQuery.ExecuteNonQuery();
            }
        }

        public List<string> GetAddressesForTransaction(Transaction transaction)
        {
            List<string> addresses = new List<string>();

            using (SQLiteCommand addrQuery = new SQLiteCommand("SELECT address FROM addresses WHERE transactionId = @transId", dbConnection))
            {
                addrQuery.Parameters.AddWithValue("@transId", transaction.Id);

                using (SQLiteDataReader reader = addrQuery.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addresses.Add(reader.GetString(0));
                    }
                }
            }

            return addresses;
        }
    }
}
