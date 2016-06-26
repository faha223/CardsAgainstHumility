using CardsAgainstHumility.Helpers;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class MainViewModel : ViewModelBase
    {
        #region Create Game Command

        internal void CreateGame()
        {
            navService.Navigate(new Uri("/CreateGamePage.xaml", UriKind.Relative));
        }

        private bool CanCreateGame()
        {
            return true;
        }

        public ICommand CreateGameCommand { get { return new ParameterlessCommandRouter(CreateGame, CanCreateGame); } }

        #endregion Create Game Command

        #region Join Game Command

        internal void JoinGame()
        {
            navService.Navigate(new Uri("/JoinPage.xaml", UriKind.Relative));
        }

        private bool CanJoinGame()
        {
            return true;
        }

        public ICommand JoinGameCommand { get { return new ParameterlessCommandRouter(JoinGame, CanJoinGame); } }

        #endregion Join Game Command

        #region Settings Command

        internal void Settings()
        {
            navService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private bool CanSettings()
        {
            return true;
        }

        public ICommand SettingsCommand { get { return new ParameterlessCommandRouter(Settings, CanSettings); } }

        #endregion Settings Command

        #region Quit Command

        internal void Quit()
        {
            Application.Current.Terminate();
        }

        private bool CanQuit()
        {
            return true;
        }

        public ICommand QuitCommand { get { return new ParameterlessCommandRouter(Quit, CanQuit); } }

        #endregion Quit Command
    }
}
