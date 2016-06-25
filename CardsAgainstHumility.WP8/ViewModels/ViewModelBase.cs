using CardsAgainstHumility.Helpers;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace CardsAgainstHumility.WP8.ViewModels
{
    class ViewModelBase : UINotifying
    {
        public static NavigationService navService;
        public static Dispatcher dispatcher;
    }
}
