using Newtonsoft.Json;

namespace Mekitamete.Daemons.Requests
{
    class MoneroCreateWalletRequest
    {
        [JsonProperty("filename")]
        public string WalletFilename { get; }
        [JsonProperty("password")]
        public string WalletPassword { get; }
        [JsonProperty("language")]
        public string SeedLanguage { get; }

        public MoneroCreateWalletRequest(string filename, string password)
        {
            WalletFilename = filename;
            WalletPassword = password;
            SeedLanguage = "English";
        }
    }
}
