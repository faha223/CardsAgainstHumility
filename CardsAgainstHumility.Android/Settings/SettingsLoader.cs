
using Android.App;
using Android.Content;
using CardsAgainstHumility.Interfaces;

namespace CardsAgainstHumility.Android.Settings
{
    class SettingsLoader : ISettingsLoader
    {
        ISharedPreferences settings;

        public SettingsLoader(Activity activity)
        {
            settings = activity.GetSharedPreferences("CardsAgainstHumility", FileCreationMode.Private);
        }

        public string GetStoredHost(string defValue)
        {
            if ((settings != null) && (settings.Contains(SettingsConstants.hostKey)))
                return settings.GetString(SettingsConstants.hostKey, defValue);
            return defValue;
        }

        public string GetStoredPlayerName(string defValue)
        {
            if ((settings != null) && (settings.Contains(SettingsConstants.playerNameKey)))
                return settings.GetString(SettingsConstants.playerNameKey, defValue);
            return defValue;
        }
    }
}