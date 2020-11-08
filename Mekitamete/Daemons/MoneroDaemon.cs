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

        /// <summary>
        /// Executes a JSON RPC call to the wallet API without any parameters.
        /// </summary>
        /// <param name="method">The API method to call.</param>
        /// <returns>Parsed JSON response.</returns>
        private JsonDocument MakeRequest(string method)
        {
            return MakeRequest(method, null);
        }

        /// <summary>
        /// Executes a JSON RPC call to the wallet API with parameters as an object. The parameters are JSON-serialized using the default serializer.
        /// </summary>
        /// <param name="method">The API method to call.</param>
        /// <param name="parameters">Parameters of the API method. These are method-specific.</param>
        /// <returns>Parsed JSON response.</returns>
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

        /// <summary>
        /// Executes a JSON RPC call to the wallet API with parameters as an object and returnes a parsed response. Throws if an error occurs.
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="method">The API method to call.</param>
        /// <param name="parameters">Parameters of the API method. These are method-specific. Can be null if no parameters are necessary.</param>
        /// <returns>An instance of the requested object, constructed by deserialization of the <b>result</b> property.</returns>
        /// <exception cref="CryptoDaemonException"></exception>
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

        /// <summary>
        /// Saves any unsaved changes into the Monero wallet file. Required after wallet-changing API calls, such as address creation.
        /// </summary>
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

        /// <summary>
        /// Creates an instance of a Monero wallet daemon and opens the default merchant wallet. If the merchant wallet does not exist, it is by default created.
        /// </summary>
        /// <param name="settings">Daemon endpoint settings.</param>
        public MoneroDaemon(RPCEndpointSettings settings)
        {
            EndpointSettings = settings;
            EndpointCredentials = new CredentialCache();
            EndpointCredentials.Add(new Uri(settings.EndpointAddress), "Digest", new NetworkCredential(settings.RPCUsername, settings.RPCPassword));

            OpenMerchantWallet();
        }
    }
}
