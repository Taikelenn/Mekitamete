using Mekitamete.Http.Requests;
using Mekitamete.Http.Responders;
using Mekitamete.Http.Responses;
using Mekitamete.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Endpoints
{
    [HttpEndpoint("/newtransaction")]
    public static class CreateTransactionEndpoint
    {
        [HttpMethod("POST")]
        public static void Post(HttpRequestArgs args)
        {
            CreateTransactionRequest request = args.GetPostData<CreateTransactionRequest>();
            if (request == null)
            {
                args.SetResponse(400, new HttpErrorResponse("Incomplete request."));
                return;
            }

            Transaction newTransaction = Transaction.CreateNewTransaction(request.Currency, request.Value, note: request.Notes);
            args.SetResponse(200, new HttpNewTransactionResponse(newTransaction));
        }
    }
}
