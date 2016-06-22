
using Android.App;
using Android.Content;
using CardsAgainstHumility.Interfaces;

namespace CardsAgainstHumility.Android
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
            if ((settings != null) && (settings.Contains("Host")))
                return settings.GetString("Host", defValue);
            return defValue;
        }

        public string GetStoredPlayerName(string defValue)
        {
            if ((settings != null) && (settings.Contains("PlayerName")))
                return settings.GetString("PlayerName", defValue);
            return defValue;
        }
    }
}