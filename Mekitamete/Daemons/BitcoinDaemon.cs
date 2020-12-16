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
    class BitcoinDaemon : ICryptoDaemon
    {
        private RPCEndpointSettings EndpointSettings { get; }
        private CredentialCache EndpointCredentials { get; }

        /// <summary>
        /// This is a usual UTF-8 encoding, just without the byte order mark which is not welcome in embedded JSON
        /// </summary>
        private static readonly Encoding Utf8Encoding = new UTF8Encoding(false);

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
        /// Executes a JSON RPC call to the wallet API with parameters as an object. The parameters are JSON-serialized using the default serializer.
        /// </summary>
        /// <param name="method">The API method to call.</param>
        /// <param name="parameters">Parameters of the API method. These are method-specific.</param>
        /// <returns>Parsed JSON response.</returns>
        private JObject MakeRequest(string method, object[] parameters)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(EndpointSettings.EndpointAddress);
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
                    jsonWriter.WriteValue("1.0");

                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue("0");

                    jsonWriter.WritePropertyName("method");
                    jsonWriter.WriteValue(method);

                    if (parameters != null)
                    {
                        jsonWriter.WritePropertyName("params");
                        jsonWriter.WriteStartArray();
                        foreach (var parameter in parameters)
                        {
                            jsonWriter.WriteValue(parameter);
                        }
                        jsonWriter.WriteEndArray();
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
        private T MakeRequest<T>(string method, params object[] parameters)
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

        private BitcoinGetTransactionResponse GetTransactionInfo(string txid)
        {
            return MakeRequest<BitcoinGetTransactionResponse>("gettransaction", txid);
        }

        public string CreateNewAddress(string label = null)
        {
            string address = MakeRequest<string>("getnewaddress", label ?? "", "bech32");
            Logger.Log("Bitcoin", $"Created a new address {address.Substring(0, Math.Min(address.Length, 10))}...");

            return address;
        }

        public List<CryptoTransaction> GetTransactions(IEnumerable<string> addresses, int minConfirmations = 0)
        {
            List<CryptoTransaction> result = new List<CryptoTransaction>();
            addresses = addresses ?? new List<string>();

            // first, get all transactions and filter them by addresses we are interested in
            IEnumerable<BitcoinListReceivedByAddressResponse> addressResponses = MakeRequest<BitcoinListReceivedByAddressResponse[]>("listreceivedbyaddress", 0);
            addressResponses = addressResponses.Where(x => addresses.Contains(x.Address));
            foreach (var response in addressResponses)
            {
                // filter out transactions below confirmation limit
                IEnumerable<BitcoinGetTransactionResponse> transactions = response.TxIds.Select(x => GetTransactionInfo(x)).Where(x => x.Confirmations >= minConfirmations);
                foreach (var transaction in transactions)
                {
                    // iterate over outputs that are ours
                    foreach (var details in transaction.Details.Where(x => x.Address == response.Address))
                    {
                        if (details.Category == "receive" || details.Category == "generate" || details.Category == "immature")
                        {
                            result.Add(new CryptoTransaction(transaction.TxId, details.Address, details.Amount, transaction.Confirmations));
                        }
                    }
                }
            }

            return result;
        }

        public BitcoinDaemon(RPCEndpointSettings settings)
        {
            Logger.Log("Bitcoin", "Initializing daemon...");

            EndpointSettings = settings;
            EndpointCredentials = new CredentialCache();
            EndpointCredentials.Add(new Uri(settings.EndpointAddress), "Basic", new NetworkCredential(settings.RPCUsername, settings.RPCPassword));

            // execute a sample RPC call to ensure that the daemon works correctly
            MakeRequest<double>("getdifficulty");

            Logger.Log("Bitcoin", "Daemon initialized");
        }
    }
}
