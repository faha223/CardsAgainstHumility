using System.Collections.Generic;

namespace CardsAgainstHumility.Interfaces
{
    public interface ISettingsLoader
    {
        string GetStoredHost(string defValue);

        string GetStoredPlayerName(string defValue);

        List<string> GetPreferredDecks(List<string> defValue);

        void SavePreferredDecks(List<string> value);
    }
}
