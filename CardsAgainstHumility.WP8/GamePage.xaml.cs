using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CardsAgainstHumility.WP8.ViewModels;
using System.Collections.ObjectModel;
using CardsAgainstHumility.GameClasses;

namespace CardsAgainstHumility.WP8
{
    public partial class GamePage : PhoneApplicationPage
    {
        GameViewModel vm;

        public GamePage()
        {
            InitializeComponent();

            vm = new GameViewModel();
            vm.PlayerHand = new ObservableCollection<WhiteCard>();
            vm.PlayerHand.Add(new WhiteCard("Another one bites the dust.", 20));
            vm.PlayerHand.Add(new WhiteCard("The biggest, blackest dick.", 20));
            vm.PlayerHand.Add(new WhiteCard("Balls.", 20));
            vm.PlayerHand.Add(new WhiteCard("Pooping back and forth forever.", 20));
            vm.PlayerHand.Add(new WhiteCard("Secretariat.", 20));

            DataContext = vm;

            Loaded += delegate
            {
                NavigationService.RemoveBackEntry();
            };
        }
    }
}