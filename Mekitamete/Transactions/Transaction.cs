using Mekitamete.Daemons;
using Mekitamete.Database;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Mekitamete.Transactions
{
    public enum TransactionCurrency : int
    {
        Bitcoin,
        Monero
    }

    public enum TransactionStatus : int
    {
        Pending,
        Completed,
        Expired,
        Cancelled
    }

    public class Transaction
    {
        public ulong Id { get; set; }
        public TransactionCurrency Currency { get; set; }
        public TransactionStatus Status { get; set; }
        public ulong PaymentAmount { get; set; }
        public ulong MinConfirmations { get; set; }
        public string Note { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }

        public List<string> Addresses
        {
            get
            {
                return MainApplication.Instance.DBConnection.GetAddressesForTransaction(this);
            }
        }

        private static readonly object IdLock = new object();
        private static readonly RNGCryptoServiceProvider cryptoRNG = new RNGCryptoServiceProvider();
        private static readonly HashSet<ulong> pendingIDs = new HashSet<ulong>();

        private static ulong GetNewTransactionId()
        {
            byte[] buf = new byte[8];
            cryptoRNG.GetBytes(buf);

            return BitConverter.ToUInt64(buf, 0) & 0x7FFFFFFFFFFFFFFF;
        }

        private static ulong GetDefaultConfirmationCount(TransactionCurrency currency)
        {
            switch (currency)
            {
                case TransactionCurrency.Bitcoin:
                    return 1;
                case TransactionCurrency.Monero:
                    return 5;
            }

            throw new ArgumentException("No data for default confirmation threshold for unknown currency", "currency");
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

        public Transaction(TransactionCurrency currency, ulong paymentAmount, ulong minConfirmations = ulong.MaxValue, string note = null, string successUrl = null, string failureUrl = null)
        {
            if (MainApplication.Instance.GetDaemonForCurrency(currency) == null)
            {
                throw new InvalidOperationException("Cannot create a transaction for a cryptocurrency which daemon is not running.");
            }

            DBConnection db = MainApplication.Instance.DBConnection;
            lock (IdLock)
            {
                do
                {
                    Id = GetNewTransactionId();
                } while (Id == 0 || pendingIDs.Contains(Id) || db.IsIDUsed(Id));

                pendingIDs.Add(Id);
            }

            Currency = currency;
            PaymentAmount = paymentAmount;
            Status = TransactionStatus.Pending;

            if (minConfirmations == ulong.MaxValue)
            {
                MinConfirmations = GetDefaultConfirmationCount(currency);
            }
            else
            {
                MinConfirmations = minConfirmations;
            }

            Note = note;
            SuccessUrl = successUrl;
            FailureUrl = failureUrl;

            db.CreateNewTransaction(this);

            lock (IdLock)
            {
                pendingIDs.Remove(Id);
            }
        }
    }
}
