using CardsAgainstHumility.GameClasses;
using CardsAgainstHumility.Helpers;
using CardsAgainstHumility.WP8.MVVM_Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class GameViewModel : ViewModelBase
    {
        private ObservableCollection<WhiteCard> _playerHand;
        public ObservableCollection<WhiteCard> PlayerHand
        {
            get
            {
                return _playerHand;
            }
            set
            {
                _playerHand = value;
                OnPropertyChanged("PlayerHand");
            }
        }

        #region Ready Command

        internal void Ready()
        {

        }

        private bool CanReady()
        {
            return true;
        }

        public ICommand ReadyCommand { get { return new ParameterlessCommandRouter(Ready, CanReady); } }

        #endregion Ready Command

        #region ConfirmCommand

        internal void Confirm()
        {

        }

        private bool CanConfirm()
        {
            return true;
        }

        public ICommand ConfirmCommand { get { return new ParameterlessCommandRouter(Confirm, CanConfirm); } }

        #endregion ConfirmCommand
    }
}
