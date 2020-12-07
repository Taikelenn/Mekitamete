using Mekitamete.Http.Responders;
using Mekitamete.Http.Responses;
using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mekitamete.Http.Endpoints
{
    [HttpEndpoint("/checktransaction/*")]
    public static class CheckTransactionEndpoint
    {
        [HttpMethod("GET")]
        public static void Get(HttpRequestArgs args)
        {
            if (!ulong.TryParse(args.UrlArguments, out ulong transactionId))
            {
                args.SetResponse(400, new HttpErrorResponse("Incorrectly formatted argument."));
                return;
            }

            Transaction t = Transaction.GetTransactionById(transactionId);
            if (t == null)
            {
                args.SetResponse(404, new HttpErrorResponse("Transaction not found."));
                return;
            }

            args.SetResponse(200, new HttpCheckTransactionResponse(t.Status));
        }
    }
}
