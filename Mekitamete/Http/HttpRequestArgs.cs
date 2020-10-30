using Mekitamete.Http.Responses;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mekitamete.Http
{
    public class HttpRequestArgs
    {
        public HttpListenerContext Context { get; }
        public IHttpJsonResponse Response { get; set; }
        public string Url { get; }

        public void SetResponse(int statusCode, IHttpJsonResponse response)
        {
            Context.Response.StatusCode = statusCode;
            Response = response;
        }

        public HttpRequestArgs(HttpListenerContext ctx)
        {
            Context = ctx;

            if (ctx.Request.RawUrl.StartsWith($"/{Settings.Instance.APIKey}/"))
            {
                Url = ctx.Request.RawUrl.Substring($"/{Settings.Instance.APIKey}".Length);
            }
        }
    }
}
