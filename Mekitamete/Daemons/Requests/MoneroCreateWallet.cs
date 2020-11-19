using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateWalletRequest
    {
        [JsonProperty("filename")]
        public string walletFilename { get; set; }
        [JsonProperty("password")]
        public string walletPassword { get; set; }
        [JsonProperty("language")]
        public string seedLanguage { get; set; }

        public MoneroCreateWalletRequest(string filename, string password)
        {
            walletFilename = filename;
            walletPassword = password;
            seedLanguage = "English";
        }
    }
}
