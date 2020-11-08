using Mekitamete.Daemons.Requests;
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

        private JsonDocument MakeRequest(string method)
        {
            return MakeRequest(method, null);
        }

        private JsonDocument MakeRequest(string method, object parameters)
        {
            string url = EndpointSettings.EndpointAddress.TrimEnd('/') + "/json_rpc";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Credentials = EndpointCredentials;
            webRequest.Timeout = 15000;

            using (var jsonWriter = new Utf8JsonWriter(webRequest.GetRequestStream()))
            {
                jsonWriter.WriteStartObject();

                jsonWriter.WriteString("jsonrpc", "2.0");
                jsonWriter.WriteString("id", "0");
                jsonWriter.WriteString("method", method);
                if (parameters != null)
                {
                    jsonWriter.WritePropertyName("params");
                    JsonSerializer.Serialize(jsonWriter, parameters);
                }

                jsonWriter.WriteEndObject();
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    return JsonDocument.Parse(sr.ReadToEnd());
                }
            }
        }

        private T MakeRequest<T>(string method, object parameters)
        {
            var resp = MakeRequest(method, parameters);
            if (resp.RootElement.TryGetProperty("result", out var jsonResult))
            {
                return JsonSerializer.Deserialize<T>(jsonResult.GetRawText());
            }

            string detailedErrorInfo = "<no error information>";
            if (resp.RootElement.TryGetProperty("error", out var errorResult))
            {
                detailedErrorInfo = errorResult.GetRawText();
            }

            throw new CryptoDaemonException($"Operation {method} failed: {detailedErrorInfo}");
        }

        private void SaveWalletFile()
        {
            MakeRequest("store");
        }

        private const string MerchantWalletName = "mekitamete_wallet";
        private void OpenMerchantWallet()
        {
            MakeRequest("close_wallet"); // close the wallet that is currently open

            var res = MakeRequest("open_wallet", new MoneroOpenWalletRequest(MerchantWalletName, EndpointSettings.WalletPassword)); // try to open the wallet

            if (res.RootElement.TryGetProperty("error", out _)) // if an error occurs, attempt to create a new wallet (Monero won't ever overwrite existing wallets anyway)
            {
                Logger.Log("Monero: merchant wallet not found, attempting to create one", Logger.MessageLevel.Warning);

                res = MakeRequest("create_wallet", new MoneroCreateWalletRequest(MerchantWalletName, EndpointSettings.WalletPassword));
                if (res.RootElement.TryGetProperty("error", out var errorElement))
                {
                    Logger.Log($"Monero: cannot create a new wallet:\n{errorElement}", Logger.MessageLevel.Error);
                    throw new CryptoDaemonException($"Failed to create a new Monero wallet: {errorElement}");
                }
            }

            Logger.Log($"Monero: opened wallet {MerchantWalletName}");
        }

        public string CreateNewAddress(string label)
        {
            var res = MakeRequest<MoneroCreateAddressResponse>("create_address", new MoneroCreateAddressRequest(label));
            Logger.Log($"Monero: created a new address {res.Address.Substring(0, Math.Min(res.Address.Length, 10))}... with index {res.AddressIndex}");

            SaveWalletFile();

            return res.Address;
        }

        public MoneroDaemon(RPCEndpointSettings settings)
        {
            EndpointSettings = settings;
            EndpointCredentials = new CredentialCache();
            EndpointCredentials.Add(new Uri(settings.EndpointAddress), "Digest", new NetworkCredential(settings.RPCUsername, settings.RPCPassword));

            OpenMerchantWallet();
        }
    }
}
