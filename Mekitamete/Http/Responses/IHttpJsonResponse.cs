using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mekitamete.Http.Responses
{
    public interface IHttpJsonResponse
    {
        public byte[] ToJsonResponse()
        {
            return JsonSerializer.SerializeToUtf8Bytes<object>(this);
        }
    }
}
