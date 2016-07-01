using CardsAgainstHumility.Interfaces;
using System.IO.IsolatedStorage;
using System;
using System.Collections.Generic;

namespace CardsAgainstHumility.WP8.Settings
{
    class SettingsLoader : ISettingsLoader
    {
        private IsolatedStorageSettings appSettings;

        public SettingsLoader()
        {
            appSettings = IsolatedStorageSettings.ApplicationSettings;
        }

        public string GetStoredHost(string defValue)
        {
            if (appSettings.Contains(SettingsConstants.serverAddressKey))
                return (string)appSettings[SettingsConstants.serverAddressKey];
            return null;
        }

        public string GetStoredPlayerName(string defValue)
        {
            if (appSettings.Contains(SettingsConstants.playerNameKey))
                return (string)appSettings[SettingsConstants.playerNameKey];
            return null;
        }

        public List<string> GetPreferredDecks(List<string> defValue)
        {
            if (appSettings.Contains(SettingsConstants.preferredDecksKey))
                return (List<string>)appSettings[SettingsConstants.preferredDecksKey];
            return defValue;
        }

        public void SavePreferredDecks(List<string> value)
        {
            if (appSettings.Contains(SettingsConstants.preferredDecksKey))
                appSettings.Remove(SettingsConstants.preferredDecksKey);
            appSettings.Add(SettingsConstants.preferredDecksKey, value);
            appSettings.Save();
        }
    }
}
