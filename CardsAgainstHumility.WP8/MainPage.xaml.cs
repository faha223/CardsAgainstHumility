using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;
using CardsAgainstHumility.WP8.Settings;

namespace CardsAgainstHumility.WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        MainViewModel vm;

        public MainPage()
        {
            InitializeComponent();

            CardsAgainstHumility.InitDefaultValues(new SettingsLoader(), new NetServices.NetServices());

            vm = new MainViewModel();
            LayoutRoot.DataContext = vm;

            Loaded += delegate
            {
                ViewModelBase.navService = NavigationService;
                ViewModelBase.dispatcher = Dispatcher;
            };
        }
    }
}