using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateWalletRequest
    {
        [JsonPropertyName("filename")]
        public string walletFilename { get; set; }
        [JsonPropertyName("password")]
        public string walletPassword { get; set; }
        [JsonPropertyName("language")]
        public string seedLanguage { get; set; }

        public MoneroCreateWalletRequest(string filename, string password)
        {
            walletFilename = filename;
            walletPassword = password;
            seedLanguage = "English";
        }
    }
}
