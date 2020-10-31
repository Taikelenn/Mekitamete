using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responders
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class HttpMethodAttribute : Attribute
    {
        public string Method { get; }

        public HttpMethodAttribute(string methodName)
        {
            Method = methodName.ToUpperInvariant();
        }
    }
}
