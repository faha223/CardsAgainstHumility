using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CardsAgainstHumility.GameClasses
{
    public class Player
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int AwesomePoints { get; set; }
        public bool IsCardCzar { get; set; }
        public bool IsReady { get; set; }
    }
}