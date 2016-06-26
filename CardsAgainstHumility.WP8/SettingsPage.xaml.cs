using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;

namespace CardsAgainstHumility.WP8
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        SettingsViewModel vm;

        public SettingsPage()
        {
            InitializeComponent();
            var vm = new SettingsViewModel();

            DataContext = vm;
        }
    }
}