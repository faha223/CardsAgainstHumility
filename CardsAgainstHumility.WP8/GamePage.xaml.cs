using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;

namespace CardsAgainstHumility.WP8
{
    public partial class GamePage : PhoneApplicationPage
    {
        GameViewModel vm;

        public GamePage()
        {
            InitializeComponent();

            vm = new GameViewModel();

            DataContext = vm;

            Loaded += delegate
            {
                NavigationService.RemoveBackEntry();
            };

            Unloaded += delegate
            {
                CardsAgainstHumility.DepartGame();
            };
        }
    }
}