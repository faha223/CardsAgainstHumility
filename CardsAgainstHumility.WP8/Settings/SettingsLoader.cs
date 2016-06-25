using CardsAgainstHumility.Interfaces;

namespace CardsAgainstHumility.WP8.Settings
{
    class SettingsLoader : ISettingsLoader
    {
        public SettingsLoader()
        {
        }

        public string GetStoredHost(string defValue)
        {
            return null; //"127.0.0.1";
        }

        public string GetStoredPlayerName(string defValue)
        {
            return null; //"Smelly Idiot";
        }
    }
}
