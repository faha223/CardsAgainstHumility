using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;
using CardsAgainstHumility.WP8.Settings;
using CardsAgainstHumility.WP8.SocketManagement;

namespace CardsAgainstHumility.WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel vm;

        public MainPage()
        {
            InitializeComponent();

            CardsAgainstHumility.InitDefaultValues(new SettingsLoader(), new SocketManager());

            vm = new MainViewModel();
            DataContext = vm;

            Loaded += delegate
            {
                ViewModelBase.navService = NavigationService;
                ViewModelBase.dispatcher = Dispatcher;
            };
        }
    }
}