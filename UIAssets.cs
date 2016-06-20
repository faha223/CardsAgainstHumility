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
using Android.Graphics;
using Android.Content.Res;

namespace CardsAgainstHumility
{
    public static class UIAssets
    {
        public static void Initialize(AssetManager Assets)
        {
            try
            {
                AppFont = Typeface.CreateFromAsset(Assets, "Helvetica-Bold.ttf");
            }
            catch { }
        }

        private static Typeface _appFont;
        public static Typeface AppFont
        {
            get
            {
                return _appFont;
            }
            private set
            {
                _appFont = value;
            }
        }
    }
}