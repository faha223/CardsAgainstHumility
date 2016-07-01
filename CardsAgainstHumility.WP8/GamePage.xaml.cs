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
using Windows.UI.Notifications;
using System;
using Windows.Data.Xml.Dom;

namespace CardsAgainstHumility.WP8
{
    public partial class GamePage : PhoneApplicationPage
    {
        GameViewModel vm;
        ToastNotifier toaster = ToastNotificationManager.CreateToastNotifier();
        ToastNotification toast;

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
            else if(toast == null)
            {
                toast = SimpleToastNotification("Please press BACK again to Leave the Game");
                toast.Dismissed += (sender, args) =>
                {
                    toast = null;
                };
                toaster.Show(toast);
                e.Cancel = true;
                return;
            }
            toaster.Hide(toast);
            toast = null;
            base.OnBackKeyPress(e);
        }

        private ToastNotification SimpleToastNotification(string ToastText)
        {
            // It is possible to start from an existing template and modify what is needed.
            // Alternatively you can construct the XML from scratch.
            var toastXml = new XmlDocument();
            var title = toastXml.CreateElement("toast");
            var visual = toastXml.CreateElement("visual");
            visual.SetAttribute("version", "1");
            visual.SetAttribute("lang", "en-US");

            // The template is set to be a ToastImageAndText01. This tells the toast notification manager what to expect next.
            var binding = toastXml.CreateElement("binding");
            binding.SetAttribute("template", "ToastImageAndText01");

            // An image element is then created under the ToastImageAndText01 XML node. The path to the image is specified
            var image = toastXml.CreateElement("image");
            image.SetAttribute("id", "1");
            image.SetAttribute("src", @"Assets/Logo.png");

            // A text element is created under the ToastImageAndText01 XML node.
            var text = toastXml.CreateElement("text");
            text.SetAttribute("id", "1");
            text.InnerText = ToastText;

            // All the XML elements are chained up together.
            title.AppendChild(visual);
            visual.AppendChild(binding);
            binding.AppendChild(image);
            binding.AppendChild(text);

            toastXml.AppendChild(title);

            // Create a ToastNotification from our XML, and send it to the Toast Notification Manager
            return new ToastNotification(toastXml);
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