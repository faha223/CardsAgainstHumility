
using Android.App;
using Android.Content;
using CardsAgainstHumility.Interfaces;
using System.Collections.Generic;

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

        public List<string> GetPreferredDecks(List<string> defValue)
        {
            if ((settings != null) && (settings.Contains(SettingsConstants.preferredDecksKey)))
            {
                var preferredDecks = settings.GetStringSet(SettingsConstants.preferredDecksKey, defValue);
                var list = new List<string>(preferredDecks.Count);
                foreach (var item in preferredDecks)
                    list.Add(item);
                return list;
            }
            return defValue;
        }

        public void SavePreferredDecks(List<string> value)
        {
            using (var editor = settings.Edit())
            {
                editor.PutStringSet(SettingsConstants.preferredDecksKey, value);
                editor.Commit();
            }
        }
    }
}