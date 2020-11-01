using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responders
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HttpEndpointAttribute : Attribute
    {
        private string Endpoint { get; }

        public HttpEndpointAttribute(string endpoint)
        {
            Endpoint = '/' + endpoint.Trim('/');
        }

        public bool ShouldServeRequest(string url)
        {
            return url.TrimEnd('/') == Endpoint;
        }
    }
}
