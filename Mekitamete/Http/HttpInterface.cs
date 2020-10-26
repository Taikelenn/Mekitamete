using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Mekitamete.Http
{
    public class HttpInterface 
    {
        private HttpListener listener;

        public HttpInterface(ushort listenPort)
        {
            listener = new HttpListener();
            listener.IgnoreWriteExceptions = true;
            listener.Prefixes.Add($"http://*:{listenPort}/");
            listener.Start();
        }
    }
}
