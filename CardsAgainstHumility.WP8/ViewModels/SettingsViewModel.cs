using CardsAgainstHumility.WP8.MVVM_Helpers;
using CardsAgainstHumility.WP8.Settings;
using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class SettingsViewModel : ViewModelBase
    {
        private IsolatedStorageSettings appSettings;

        private string playerName;
        public string PlayerName
        {
            get
            {
                return playerName;
            }
            set
            {
                if (playerName != value)
                {
                    playerName = value;
                    OnPropertyChanged("PlayerName");
                }
            }
        }

        private string serverAddress;
        public string ServerAddress
        {
            get
            {
                return serverAddress;
            }
            set
            {
                if (serverAddress != value)
                {
                    serverAddress = value;
                    OnPropertyChanged("ServerAddress");
                }
            }
        }

        #region Save Changes Command

        internal void SaveChanges()
        {
            appSettings[SettingsConstants.playerNameKey] = PlayerName;
            appSettings[SettingsConstants.serverAddressKey] = ServerAddress;
            appSettings.Save();

            CardsAgainstHumility.PlayerName = PlayerName;
            CardsAgainstHumility.Host = ServerAddress;
            navService.GoBack();
        }

        private bool CanSaveChanges()
        {
            return true;
        }

        public ICommand SaveChangesCommand { get { return new ParameterlessCommandRouter(SaveChanges, CanSaveChanges); } }

        #endregion Save Changes Command

        public SettingsViewModel() : base()
        {
            appSettings = IsolatedStorageSettings.ApplicationSettings;
            PlayerName = CardsAgainstHumility.PlayerName;
            ServerAddress = CardsAgainstHumility.Host;
        }
    }
}
