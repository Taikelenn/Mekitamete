using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Http.Responses
{
    public class HttpGetTransactionResponse : HttpJsonResponse
    {
        public ulong Id { get; private set; }
        public TransactionCurrency Currency { get; private set; }
        public string Status { get; private set; }
        public long ConfirmedValue { get; private set; }
        public long UnconfirmedValue { get; private set; }
        public long TotalValue { get; private set; }
        public int ConfirmationsReceived { get; private set; }
        public int MinConfirmations { get; private set; }
        public List<string> Addresses { get; private set; }

        public HttpGetTransactionResponse(Transaction t)
        {
            Id = t.Id;
            Currency = t.Currency;
            Status = t.Status.ToString();
            TotalValue = t.PaymentAmount;
            MinConfirmations = t.MinConfirmations;
            Addresses = t.Addresses;

            var received = t.GetReceivedAmount();
            ConfirmedValue = received.Confirmed;
            UnconfirmedValue = received.Unconfirmed;

            ConfirmationsReceived = t.GetConfirmationCount();
        }
    }
}
