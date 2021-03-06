﻿using Newtonsoft.Json;

namespace Mekitamete.Daemons.Requests
{
    class MoneroOpenWalletRequest
    {
        [JsonProperty("filename")]
        public string WalletFilename { get; }
        [JsonProperty("password")]
        public string WalletPassword { get; }

        public MoneroOpenWalletRequest(string filename, string password)
        {
            WalletFilename = filename;
            WalletPassword = password;
        }
    }
}
