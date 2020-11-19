using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateWalletRequest
    {
        [JsonProperty("filename")]
        public string walletFilename { get; }
        [JsonProperty("password")]
        public string walletPassword { get; }
        [JsonProperty("language")]
        public string seedLanguage { get; }

        public MoneroCreateWalletRequest(string filename, string password)
        {
            walletFilename = filename;
            walletPassword = password;
            seedLanguage = "English";
        }
    }
}
