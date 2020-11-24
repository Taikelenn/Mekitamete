using Mekitamete.Http.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Mekitamete.Http
{
    public class HttpRequestArgs
    {
        public HttpListenerContext Context { get; }
        public HttpJsonResponse Response { get; set; }
        public string Url { get; }

        public void SetResponse(int statusCode, HttpJsonResponse response)
        {
            Context.Response.StatusCode = statusCode;
            Response = response;
        }

        public T GetPostData<T>()
        {
            string input = "";
            using (StreamReader sr = new StreamReader(Context.Request.InputStream))
            {
                input = sr.ReadToEnd();
            }

            return JsonConvert.DeserializeObject<T>(input);
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
