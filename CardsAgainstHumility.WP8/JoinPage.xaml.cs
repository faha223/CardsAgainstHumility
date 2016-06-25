using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;

namespace CardsAgainstHumility.WP8
{
    public partial class JoinPage : PhoneApplicationPage
    {
        LobbyViewModel vm;
        public JoinPage()
        {
            InitializeComponent();
            vm = new LobbyViewModel();

            DataContext = vm;
        }
    }
}