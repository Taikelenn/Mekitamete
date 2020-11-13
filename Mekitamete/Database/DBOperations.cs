﻿using Mekitamete.Transactions;
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
                "tag TEXT NOT NULL DEFAULT 'global'," + // "tag" is currently not implemented in the program
                "currency INTEGER NOT NULL," +
                "value INTEGER NOT NULL," +
                "minConfirmations INTEGER NOT NULL DEFAULT 1," +
                "note TEXT," +
                "successUrl TEXT," +
                "failureUrl TEXT," +
                "PRIMARY KEY (id, tag)" +
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
            // we're not worried about SQL injection here because we're only allowing numeric values
            using (SQLiteCommand query = new SQLiteCommand($"SELECT 1 FROM transactions WHERE id = {id}", dbConnection))
            {
                return query.ExecuteScalar() != null;
            }
        }

        public void CreateTransaction(Transaction transaction)
        {
            using (SQLiteCommand insertQuery = new SQLiteCommand("INSERT INTO transactions(id, currency, value, minConfirmations, note, successUrl, failureUrl)" +
                "VALUES (@newId, @currency, @value, @minConf, @note, @successUrl, @failureUrl)", dbConnection))
            {
                insertQuery.Parameters.AddWithValue("@newId", transaction.Id);
                insertQuery.Parameters.AddWithValue("@currency", transaction.Currency);
                insertQuery.Parameters.AddWithValue("@value", transaction.PaymentAmount);
                insertQuery.Parameters.AddWithValue("@minConf", transaction.MinConfirmations);
                insertQuery.Parameters.AddWithValue("@note", transaction.Note);
                insertQuery.Parameters.AddWithValue("@successUrl", transaction.SuccessUrl);
                insertQuery.Parameters.AddWithValue("@failureUrl", transaction.FailureUrl);
                insertQuery.ExecuteNonQuery();
            }
        }
    }
}
