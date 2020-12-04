using Mekitamete.Daemons;
using Mekitamete.Database;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mekitamete.Transactions
{
    public enum TransactionCurrency : int
    {
        Bitcoin = 0,
        Monero = 1
    }

    public enum TransactionStatus : int
    {
        Pending = 0,
        Completed = 1,
        Expired = 2,
        Cancelled = 3
    }

    public partial class Transaction
    {
        public ulong Id { get; private set; }
        public TransactionCurrency Currency { get; private set; }
        public TransactionStatus Status { get; private set; }
        public long PaymentAmount { get; private set; }
        public int MinConfirmations { get; private set; }
        public string Note { get; private set; }
        public string SuccessUrl { get; private set; }
        public string FailureUrl { get; private set; }

        public List<string> Addresses
        {
            get
            {
                return MainApplication.Instance.DBConnection.GetAddressesForTransaction(this);
            }
        }

        public string AddNewAddress()
        {
            ICryptoDaemon daemon = MainApplication.Instance.GetDaemonForCurrency(Currency);
            if (daemon == null)
            {
                throw new InvalidOperationException("Cannot add a new address for a cryptocurrency which daemon is not running.");
            }

            string addr = daemon.CreateNewAddress($"meki-{Id}");

            MainApplication.Instance.DBConnection.AddNewAddress(this, addr);
            return addr;
        }

        internal static Transaction GetTransactionById(ulong transactionId)
        {
            DBConnection db = MainApplication.Instance.DBConnection;
            Transaction t = db.GetTransaction(transactionId);
            if (t == null)
            {
                return null;
            }

            if (MainApplication.Instance.GetDaemonForCurrency(t.Currency) == null)
            {
                throw new InvalidOperationException("Cannot get a transaction for a cryptocurrency which daemon is not running.");
            }

            return t;
        }
    }
}
