using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;
using Microsoft.Xna.Framework.Input.Touch;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;

namespace CardsAgainstHumility.WP8
{
    public partial class GamePage : PhoneApplicationPage
    {
        GameViewModel vm;

        public GamePage()
        {
            InitializeComponent();
            _drawerLayout.InitializeDrawerLayout();

            vm = new GameViewModel();
            _drawerLayout.DrawerOpened += sender =>
            {
                vm.SetDrawerLayoutOpen(true);
            };

            _drawerLayout.DrawerClosed += sender =>
            {
                vm.SetDrawerLayoutOpen(false);
            };

            vm.CloseDrawer += (sender, args) =>
            {
                _drawerLayout.CloseDrawer();
            };

            DataContext = vm;

            TouchPanel.EnabledGestures = GestureType.Flick;
            ManipulationCompleted += manipulationCompleted;

            Loaded += delegate
            {
                NavigationService.RemoveBackEntry();
            };

            Unloaded += delegate
            {
                CardsAgainstHumility.DepartGame();
            };
        }

        private void manipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if(TouchPanel.IsGestureAvailable)
            {
                var gesture = TouchPanel.ReadGesture();
                switch(gesture.GestureType)
                {
                    case GestureType.Flick:
                        if (IsOpenPlayerListGesture(gesture))
                            vm.IsPlayerListOpen = true;
                        else if (IsClosePlayerListGesture(gesture))
                            vm.IsPlayerListOpen = false;
                        break;
                    default:
                        break;
                }
            }
        }

        bool IsOpenPlayerListGesture(GestureSample gesture)
        {
            return (gesture.Delta.X > 0) && (gesture.Position.X == 0);
        }

        bool IsClosePlayerListGesture(GestureSample gesture)
        {
            return (gesture.Delta.X < 0);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (vm.BackButtonPressed())
            {
                e.Cancel = true;
                return;
            }
            base.OnBackKeyPress(e);
        }

        public static string GetInlineList(TextBlock element)
        {
            if (element != null)
                return element.GetValue(ArticleContentProperty) as string;
            return string.Empty;
        }

        public static void SetInlineList(TextBlock element, string value)
        {
            if (element != null)
                element.SetValue(ArticleContentProperty, value);
        }

        public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "InlineList",
                typeof(List<Inline>),
                typeof(GamePage),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            var tb = obj as TextBlock;
            if (tb != null)
            {
                // clear previous inlines
                tb.Inlines.Clear();

                // add new inlines
                var inlines = e.NewValue as List<Inline>;
                if (inlines != null)
                {
                    inlines.ForEach(inl => tb.Inlines.Add((inl)));
                }
            }
        }
    }
}