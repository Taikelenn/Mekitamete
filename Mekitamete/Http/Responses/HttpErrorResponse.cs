using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responses
{
    public class HttpErrorResponse : IHttpJsonResponse
    {
        public string Status { get; }
        public string Message { get; }

        public HttpErrorResponse(string message)
        {
            Status = "error";
            Message = message;
        }
    }
}
