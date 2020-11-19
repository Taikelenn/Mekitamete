using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mekitamete.Daemons.Requests
{
    class MoneroOpenWalletRequest
    {
        [JsonProperty("filename")]
        public string walletFilename { get; }
        [JsonProperty("password")]
        public string walletPassword { get; }

        public MoneroOpenWalletRequest(string filename, string password)
        {
            walletFilename = filename;
            walletPassword = password;
        }
    }
}
