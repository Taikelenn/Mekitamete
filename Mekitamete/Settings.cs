using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mekitamete
{
    // public setters are required for System.Text.JSON to work correctly; the setters should preferably be private, but this must wait until...
    // ...we switch to Newtonsoft.JSON or .NET 5.0 gets released with the JsonInclude attribute

    public class RPCEndpointSettings
    {
        public string EndpointAddress { get; set; } = "http://127.0.0.1:10000";
        public string RPCUsername { get; set; } = "rpcuser";
        public string RPCPassword { get; set; } = "rpcpass";
        public string WalletPassword { get; set; } = null;
    }

    public class Settings
    {
        public static Settings Instance { get; } = LoadSettings();

        private const string SettingsFileName = "mekitamete.conf";
        private static Settings LoadSettings()
        {
            Settings s;
            var serializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            s = File.Exists(SettingsFileName) ? JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFileName, Encoding.UTF8), serializationOptions) : new Settings();
            File.WriteAllText(SettingsFileName, JsonSerializer.Serialize(s, serializationOptions), Encoding.UTF8);

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

            if (BitcoinDaemon == null && MoneroDaemon == null)
            {
                errorMsg = "neither Bitcoin nor Monero daemon data was specified";
            }

            return errorMsg == "";
        }

        public ushort ServerPort { get; set; } = 48881;
        public string APIKey { get; set; } = "0ce3d42759116e3cdedebeb5c1d53c81f4a814dadcf9d11b";
        public RPCEndpointSettings BitcoinDaemon { get; set; } = new RPCEndpointSettings();
        public RPCEndpointSettings MoneroDaemon { get; set; } = new RPCEndpointSettings();
    }
}
