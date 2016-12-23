﻿using BolTDL;
using Newtonsoft.Json;

namespace BolTDLConsole
{
    class BolTDLConsoleSettings
    {
        public bool useColors { get; set; }
		public bool useDescriptions { get; set; }
		public string username { get; set; }

        public static readonly string fileName = "BolTDLConsoleSettings.JSON";

        [JsonConstructor]
        public BolTDLConsoleSettings()
        {
        }

        public BolTDLConsoleSettings(bool colors, bool descriptions)
        {
            useColors = true;
            useDescriptions = true;
        }

		public BolTDLConsoleSettings(bool colors, bool descriptions, string usn)
		{
			useColors = true;
			useDescriptions = true;
			username = usn;
		}

        public static BolTDLConsoleSettings SettingsFromJson(string json)
        {
            return JsonConvert.DeserializeObject<BolTDLConsoleSettings>(json);
        }

        public void ExportSettings()
        {
            string settings = JsonConvert.SerializeObject(this);
            DataHandler.ExportSettings(fileName, settings);
        }
    }
}
