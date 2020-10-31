using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responders
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HttpEndpointAttribute : Attribute
    {
        public string Endpoint { get; }

        public HttpEndpointAttribute(string endpoint)
        {
            if (!endpoint.StartsWith('/'))
            {
                endpoint = '/' + endpoint;
            }

            Endpoint = endpoint;
        }
    }
}
