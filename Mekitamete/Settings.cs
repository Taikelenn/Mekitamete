using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mekitamete
{
    public class RPCEndpointSettings
    {
        [JsonProperty]
        public string EndpointAddress { get; private set; } = "http://127.0.0.1:10000";
        [JsonProperty]
        public string RPCUsername { get; private set; } = "rpcuser";
        [JsonProperty]
        public string RPCPassword { get; private set; } = "rpcpass";
        [JsonProperty]
        public string WalletPassword { get; private set; } = null;
    }

    public class Settings
    {
        public static Settings Instance { get; } = LoadSettings();

        private const string SettingsFileName = "mekitamete.conf";
        private static Settings LoadSettings()
        {
            Settings s;

            s = File.Exists(SettingsFileName) ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFileName, Encoding.UTF8)) : new Settings();
            File.WriteAllText(SettingsFileName, JsonConvert.SerializeObject(s, Formatting.Indented), Encoding.UTF8);

            if (!s.ValidateSettings(out string errorMsg))
            {
                throw new InvalidOperationException($"The configuration file is invalid: {errorMsg}\nPlease adjust the faulty configuration fields.");
            }

            return s;
        }

        private bool ValidateSettings(out string errorMsg)
        {
            errorMsg = "";
            if (String.IsNullOrWhiteSpace(APIKey))
            {
                errorMsg = "API key is empty";
            }

            return errorMsg == "";
        }

        [JsonProperty]
        public ushort ServerPort { get; private set; } = 48881;
        [JsonProperty]
        public string APIKey { get; private set; } = "0ce3d42759116e3cdedebeb5c1d53c81f4a814dadcf9d11b";
        [JsonProperty]
        public RPCEndpointSettings BitcoinDaemon { get; private set; } = new RPCEndpointSettings();
        [JsonProperty]
        public RPCEndpointSettings MoneroDaemon { get; private set; } = new RPCEndpointSettings();
    }
}
