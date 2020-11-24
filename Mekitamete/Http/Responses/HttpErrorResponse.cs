using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responses
{
    public class HttpErrorResponse : HttpJsonResponse
    {
        public string Message { get; }

        public HttpErrorResponse(string message)
        {
            Status = "error";
            Message = message;
        }
    }
}
