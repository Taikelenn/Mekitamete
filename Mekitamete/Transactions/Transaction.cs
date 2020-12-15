using Mekitamete.Daemons;
using Mekitamete.Database;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public class ReceivedAmount
        {
            public long Total { get; private set; }
            public long Confirmed { get; private set; }
            public long Unconfirmed { get { return Total - Confirmed; } }

            public ReceivedAmount(long confirmed, long total)
            {
                Confirmed = confirmed;
                Total = total;
            }
        }

        public ulong Id { get; private set; }
        public TransactionCurrency Currency { get; private set; }
        public TransactionStatus Status { get; private set; }
        public long PaymentAmount { get; private set; }
        public int MinConfirmations { get; private set; }
        public string Note { get; private set; }
        public string SuccessUrl { get; private set; }
        public string FailureUrl { get; private set; }

        private ICryptoDaemon AssociatedDaemon { get; }
        private object TransactionLock { get; }

        public List<string> Addresses
        {
            get
            {
                return MainApplication.Instance.DBConnection.GetAddressesForTransaction(this);
            }
        }

        public string AddNewAddress()
        {
            string addr = AssociatedDaemon.CreateNewAddress($"meki-{Id}");

            MainApplication.Instance.DBConnection.AddNewAddress(this, addr);
            return addr;
        }

        public ReceivedAmount GetReceivedAmount()
        {
            return new ReceivedAmount(AssociatedDaemon.GetReceivedBalance(Addresses, MinConfirmations), AssociatedDaemon.GetReceivedBalance(Addresses));
        }

        public int GetConfirmationCount()
        {
            return AssociatedDaemon.GetTransactions(Addresses).Select(x => x.Confirmations).DefaultIfEmpty().Min();
        }

        private bool SetTransactionStatus(TransactionStatus newStatus)
        {
            lock (TransactionLock) // lock this particular transaction to avoid multiple completions/cancellations
            {
                if (Status != TransactionStatus.Pending)
                {
                    return false;
                }

                Status = newStatus;
            }

            return true;
        }

        public void CompleteTransaction()
        {
            if (SetTransactionStatus(TransactionStatus.Completed))
            {
                MainApplication.Instance.DBConnection.UpdateTransactionStatus(this);
            }
        }

        public void CancelTransaction()
        {
            if (SetTransactionStatus(TransactionStatus.Cancelled))
            {
                MainApplication.Instance.DBConnection.UpdateTransactionStatus(this);
            }
        }

        public void UpdateTransaction()
        {
            if (Status == TransactionStatus.Pending && GetReceivedAmount().Confirmed >= PaymentAmount)
            {
                CompleteTransaction();
            }
        }

        internal static Transaction GetTransactionById(ulong transactionId)
        {
            return MainApplication.Instance.DBConnection.GetTransaction(transactionId);
        }
    }
}
