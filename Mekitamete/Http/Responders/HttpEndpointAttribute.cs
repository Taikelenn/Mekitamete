using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responders
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HttpEndpointAttribute : Attribute
    {
        private string Endpoint { get; }
        public bool UrlContainsArguments { get { return Endpoint.EndsWith('*'); } }

        public HttpEndpointAttribute(string endpoint)
        {
            Endpoint = '/' + endpoint.Trim('/');
        }

        public bool ShouldServeRequest(string url)
        {
            if (UrlContainsArguments)
            {
                return url.StartsWith(Endpoint.TrimEnd('*', '/') + "/");
            }

            return url.TrimEnd('/') == Endpoint;
        }

        public string GetUrlArguments(string url)
        {
            if (!UrlContainsArguments)
            {
                throw new InvalidOperationException("URL arguments can be retrieved only for endpoints ending with an asterisk.");
            }

            return url.Substring(Endpoint.TrimEnd('*').Length).TrimEnd('/');
        }
    }
}
