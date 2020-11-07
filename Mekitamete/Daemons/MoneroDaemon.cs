using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Mekitamete.Daemons
{
    class MoneroDaemon : ICryptoDaemon
    {
        private RPCEndpointSettings EndpointSettings { get; }
        private CredentialCache EndpointCredentials { get; }

        private string MakeRequest(string method, object parameters)
        {
            string url = EndpointSettings.EndpointAddress.TrimEnd('/') + "/json_rpc";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Credentials = EndpointCredentials;

            using (var jsonWriter = new Utf8JsonWriter(webRequest.GetRequestStream()))
            {
                jsonWriter.WriteStartObject();

                jsonWriter.WriteString("jsonrpc", "2.0");
                jsonWriter.WriteString("id", "0");
                jsonWriter.WriteString("method", method);
                if (parameters != null)
                {
                    jsonWriter.WriteString("params", JsonSerializer.Serialize(parameters));
                }

                jsonWriter.WriteEndObject();
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        public MoneroDaemon(RPCEndpointSettings settings)
        {
            EndpointSettings = settings;
            EndpointCredentials = new CredentialCache();
            EndpointCredentials.Add(new Uri(settings.EndpointAddress), "Digest", new NetworkCredential(settings.RPCUsername, settings.RPCPassword));

            Logger.Log(MakeRequest("get_height", null));
        }
    }
}
