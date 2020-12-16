using Mekitamete.Http.Responders;
using Mekitamete.Http.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Endpoints
{
    [HttpEndpoint("/status")]
    public static class StatusEndpoint
    {
        [HttpMethod("GET")]
        public static void Get(HttpRequestArgs args)
        {
            args.SetResponse(200, new HttpStatusCheckResponse());
        }
    }
}
