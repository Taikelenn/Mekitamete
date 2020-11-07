using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Mekitamete
{
    public class RPCEndpointSettings
    {
        public string EndpointAddress { get; }
        public string RPCUsername { get; }
        public string RPCPassword { get; }
        public string WalletPassword { get; }

        public RPCEndpointSettings()
        {
            EndpointAddress = "http://127.0.0.1:10000";
            RPCUsername = "rpcuser";
            RPCPassword = "rpcpass";
            WalletPassword = null;
        }
    }

    public class Settings
    {
        public static Settings Instance { get; } = LoadSettings();

        private const string SettingsFileName = "mekitamete.conf";
        private static Settings LoadSettings()
        {
            Settings s;

            if (File.Exists(SettingsFileName))
            {
                s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(SettingsFileName, Encoding.UTF8));
            }
            else
            {
                s = new Settings();
            }

            var serializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };

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

        private Settings()
        {
            ServerPort = 48881;
            APIKey = "0ce3d42759116e3cdedebeb5c1d53c81f4a814dadcf9d11b";
            BitcoinDaemon = new RPCEndpointSettings();
            MoneroDaemon = new RPCEndpointSettings();
        }

        public ushort ServerPort { get; }
        public string APIKey { get; }
        public RPCEndpointSettings BitcoinDaemon { get; }
        public RPCEndpointSettings MoneroDaemon { get; }
    }
}
