using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responses
{
    public class HttpJsonResponse
    {
        public string Status { get; protected set; } = "ok";

        public byte[] ToJsonResponse()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
