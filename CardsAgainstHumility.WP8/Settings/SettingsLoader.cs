using CardsAgainstHumility.Interfaces;
using System.IO.IsolatedStorage;

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
    }
}
