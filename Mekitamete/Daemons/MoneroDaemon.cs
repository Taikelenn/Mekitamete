using Mekitamete.Daemons.Requests;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Mekitamete.Daemons
{
    class MoneroDaemon : ICryptoDaemon
    {
        private RPCEndpointSettings EndpointSettings { get; }
        private CredentialCache EndpointCredentials { get; }

        private const long RefreshTickRate = 7000; // refresh every 7 seconds
        private long LastRefreshTick { get; set; }

        /// <summary>
        /// Executes a JSON RPC call to the wallet API without any parameters.
        /// </summary>
        /// <param name="method">The API method to call.</param>
        /// <returns>Parsed JSON response.</returns>
        private JObject MakeRequest(string method)
        {
            return MakeRequest(method, null);
        }

        /// <summary>
        /// This is a usual UTF-8 encoding, just without the byte order mark which is not welcome in embedded JSON
        /// </summary>
        private static readonly Encoding Utf8Encoding = new UTF8Encoding(false);

        /// <summary>
        /// Executes a JSON RPC call to the wallet API with parameters as an object. The parameters are JSON-serialized using the default serializer.
        /// </summary>
        /// <param name="method">The API method to call.</param>
        /// <param name="parameters">Parameters of the API method. These are method-specific.</param>
        /// <returns>Parsed JSON response.</returns>
        private JObject MakeRequest(string method, object parameters)
        {
            if (Environment.TickCount64 - LastRefreshTick > RefreshTickRate)
            {
                Logger.Log("Monero", "Refreshing wallet...");

                LastRefreshTick = Environment.TickCount64;
                MakeRequest("refresh");
            }

            string url = EndpointSettings.EndpointAddress.TrimEnd('/') + "/json_rpc";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Credentials = EndpointCredentials;
            webRequest.Timeout = 15000;

            using (var streamWriter = new StreamWriter(webRequest.GetRequestStream(), Utf8Encoding))
            {
                using (var jsonWriter = new JsonTextWriter(streamWriter))
                {
                    jsonWriter.WriteStartObject();

                    jsonWriter.WritePropertyName("jsonrpc");
                    jsonWriter.WriteValue("2.0");

                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue("0");

                    jsonWriter.WritePropertyName("method");
                    jsonWriter.WriteValue(method);

                    if (parameters != null)
                    {
                        jsonWriter.WritePropertyName("params");
                        JsonSerializer.CreateDefault().Serialize(jsonWriter, parameters);
                    }

                    jsonWriter.WriteEndObject();
                }
            }

            using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                using (StreamReader sr = new StreamReader(webResponse.GetResponseStream()))
                {
                    return JObject.Parse(sr.ReadToEnd());
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
            var successResult = resp["result"];
            if (successResult == null) // call failed
            {
                string detailedErrorInfo = "<no error information>";
                var errorObj = resp["error"];

                if (errorObj != null)
                {
                    detailedErrorInfo = errorObj.ToString();
                }

                throw new CryptoDaemonException($"Operation {method} failed: {detailedErrorInfo}");
            }

            return successResult.ToObject<T>();
        }

        /// <summary>
        /// Saves any unsaved changes into the Monero wallet file.
        /// </summary>
        private void SaveWalletFile()
        {
            MakeRequest("store");
        }

        private uint GetAddressIndex(string address)
        {
            return MakeRequest<MoneroGetAddressIndexResponse>("get_address_index", new MoneroGetAddressIndexRequest(address)).IndexMinor;
        }

        private const string MerchantWalletName = "mekitamete_wallet";
        private void OpenMerchantWallet()
        {
            MakeRequest("close_wallet"); // close the wallet that is currently open

            var response = MakeRequest("open_wallet", new MoneroOpenWalletRequest(MerchantWalletName, EndpointSettings.WalletPassword)); // try to open the wallet

            if (response.ContainsKey("error")) // if an error occurs, attempt to create a new wallet (Monero won't ever overwrite existing wallets anyway)
            {
                Logger.Log("Monero", "Merchant wallet not found, attempting to create one", Logger.MessageLevel.Warning);

                response = MakeRequest("create_wallet", new MoneroCreateWalletRequest(MerchantWalletName, EndpointSettings.WalletPassword));
                if (response.ContainsKey("error"))
                {
                    string errorData = response["error"].ToString();

                    Logger.Log("Monero", $"Cannot create a new wallet:\n{errorData}", Logger.MessageLevel.Error);
                    throw new CryptoDaemonException($"Failed to create a new Monero wallet: {errorData}");
                }
            }

            Logger.Log("Monero", $"Opened wallet {MerchantWalletName}");
        }

        public string CreateNewAddress(string label = null)
        {
            var response = MakeRequest<MoneroCreateAddressResponse>("create_address", new MoneroCreateAddressRequest(label));
            Logger.Log("Monero", $"Created a new address {response.Address.Substring(0, Math.Min(response.Address.Length, 10))}... with index {response.AddressIndex}");

            SaveWalletFile();

            return response.Address;
        }

        public List<CryptoTransaction> GetTransactions(IEnumerable<string> addresses = null, int minConfirmations = 0)
        {
            List<CryptoTransaction> result = new List<CryptoTransaction>();

            addresses = addresses ?? new List<string>();
            var response = MakeRequest<MoneroGetTransfersResponse>("get_transfers", new MoneroGetTransfersRequest(addresses.Select(x => GetAddressIndex(x))));

            if (response.IncomingTransfers != null)
            {
                foreach (var item in response.IncomingTransfers)
                {
                    result.Add(new CryptoTransaction(item.TxId, item.Address, (long)item.Value, (int)item.Confirmations));
                }
            }
            if (response.PoolTransfers != null)
            {
                foreach (var item in response.PoolTransfers)
                {
                    result.Add(new CryptoTransaction(item.TxId, item.Address, (long)item.Value, 0));
                }
            }

            return result.Where(x => x.Confirmations >= minConfirmations).ToList();
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
            LastRefreshTick = 0; // refresh on the next API call
        }
    }
}
