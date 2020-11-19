using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Http.Responses
{
    public interface IHttpJsonResponse
    {
        public byte[] ToJsonResponse()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
