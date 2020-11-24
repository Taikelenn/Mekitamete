using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responses
{
    public class HttpNewTransactionResponse : HttpJsonResponse
    {
        public ulong TransactionId { get; }

        public HttpNewTransactionResponse(Transaction transaction)
        {
            TransactionId = transaction.Id;
        }
    }
}
