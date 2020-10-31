using Mekitamete.Http.Responders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Endpoints
{
    [HttpEndpoint("/")]
    class MainEndpoint
    {
        [HttpMethod("GET")]
        public static void Get(HttpRequestArgs args)
        {
            args.SetResponse(200, null);
        }
    }
}
