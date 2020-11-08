using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Mekitamete.Daemons.Requests
{
    class MoneroOpenWalletRequest
    {
        [JsonPropertyName("filename")]
        public string walletFilename { get; set; }
        [JsonPropertyName("password")]
        public string walletPassword { get; set; }

        public MoneroOpenWalletRequest(string filename, string password)
        {
            walletFilename = filename;
            walletPassword = password;
        }
    }
}
