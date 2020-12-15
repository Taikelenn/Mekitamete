using Mekitamete.Database;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Transactions
{
    public partial class Transaction
    {
        private static readonly object IdLock = new object();
        private static readonly RNGCryptoServiceProvider cryptoRNG = new RNGCryptoServiceProvider();
        private static readonly HashSet<ulong> pendingIDs = new HashSet<ulong>();

        private Transaction(TransactionCurrency currency)
        {
            TransactionLock = new object();
            Currency = currency;
            AssociatedDaemon = MainApplication.Instance.GetDaemonForCurrency(currency);

            if (AssociatedDaemon == null)
            {
                throw new InvalidOperationException("Cannot create a transaction for a cryptocurrency which daemon is not running.");
            }
        }

        private static ulong GetNewTransactionId()
        {
            byte[] buf = new byte[8];
            cryptoRNG.GetBytes(buf);

            return BitConverter.ToUInt64(buf, 0) & 0x7FFFFFFFFFFFFFFF;
        }

        public static Transaction CreateNewTransaction(TransactionCurrency currency, long paymentAmount, int minConfirmations = int.MaxValue, string note = null, string successUrl = null, string failureUrl = null)
        {
            DBConnection db = MainApplication.Instance.DBConnection;
            Transaction t = new Transaction(currency);

            lock (IdLock)
            {
                do
                {
                    t.Id = GetNewTransactionId();
                } while (t.Id == 0 || pendingIDs.Contains(t.Id) || db.IsIDUsed(t.Id));

                pendingIDs.Add(t.Id);
            }

            t.PaymentAmount = paymentAmount;
            t.Status = TransactionStatus.Pending;

            if (minConfirmations == int.MaxValue)
            {
                t.MinConfirmations = currency.GetDefaultConfirmationCount();
            }
            else
            {
                t.MinConfirmations = minConfirmations;
            }

            t.Note = note;
            t.SuccessUrl = successUrl;
            t.FailureUrl = failureUrl;

            db.CreateNewTransaction(t);

            lock (IdLock)
            {
                pendingIDs.Remove(t.Id);
            }

            Logger.Log("Transactions", $"Created a new transaction for {currency.PrettyPrintedValue(t.PaymentAmount)} with ID {t.Id}");
            return t;
        }

        internal static Transaction CreateFromDatabaseEntry(SQLiteDataReader reader)
        {
            TransactionCurrency currency = (TransactionCurrency)reader.GetInt32(2);
            Transaction t = new Transaction(currency);

            t.Id = (ulong)reader.GetInt64(0);
            t.Status = (TransactionStatus)reader.GetInt32(1);
            t.PaymentAmount = reader.GetInt64(3);
            t.MinConfirmations = reader.GetInt32(4);

            if (!reader.IsDBNull(5))
                t.Note = reader.GetString(5);

            if (!reader.IsDBNull(6))
                t.SuccessUrl = reader.GetString(6);

            if (!reader.IsDBNull(7))
                t.FailureUrl = reader.GetString(7);

            return t;
        }
    }
}
