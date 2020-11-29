using Mekitamete.Http.Responders;
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
            
        }
    }
}
