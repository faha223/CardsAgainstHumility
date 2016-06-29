﻿using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using CardsAgainstHumility.WP8.ViewModels;
using Microsoft.Xna.Framework.Input.Touch;
using System.Windows.Input;
using System.ComponentModel;

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
    }
}