using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Http.Responses
{
    public class HttpCheckTransactionResponse : HttpJsonResponse
    {
        public string Status { get; private set; }

        public HttpCheckTransactionResponse(TransactionStatus status)
        {
            Status = status.ToString();
        }
    }
}
