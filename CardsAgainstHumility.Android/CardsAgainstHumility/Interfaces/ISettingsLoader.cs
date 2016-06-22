namespace CardsAgainstHumility.Interfaces
{
    public interface ISettingsLoader
    {
        string GetStoredHost(string defValue);

        string GetStoredPlayerName(string defValue);
    }
}
