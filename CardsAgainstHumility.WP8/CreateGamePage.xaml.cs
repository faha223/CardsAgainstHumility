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

namespace CardsAgainstHumility.WP8
{
    public partial class CreateGamePage : PhoneApplicationPage
    {
        CreateGameViewModel vm;

        public CreateGamePage()
        {
            InitializeComponent();
            vm = new CreateGameViewModel();

            DataContext = vm;
        }
    }
}