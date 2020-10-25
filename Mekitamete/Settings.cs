using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Mekitamete
{
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
            return s;
        }

        private Settings()
        {
            ServerPort = 48881;
            APIKey = "0ce3d42759116e3cdedebeb5c1d53c81f4a814dadcf9d11b";
        }

        public ushort ServerPort { get; }
        public string APIKey { get; }
    }
}
