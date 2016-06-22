using Android.Graphics;
using Android.Content.Res;
using Android.Widget;
using Android.Views;
using CardsAgainstHumility.GameClasses;
using Android.Text;
using Android.Util;

namespace CardsAgainstHumility.Android.UIHelpers
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

        public static void PrepareBlackCard(View view, BlackCard currentQuestion)
        {
            var text = view.FindViewById<TextView>(Resource.Id.bc_CardText);
            if (AppFont != null)
            {
                var txtlogo = view.FindViewById<TextView>(Resource.Id.bc_logo_text);
                txtlogo.SetTypeface(AppFont, TypefaceStyle.Normal);
                text.SetTypeface(AppFont, TypefaceStyle.Normal);
            }

            if (currentQuestion != null)
            {
                text.TextFormatted = Html.FromHtml(currentQuestion.Text);
                text.SetTextSize(ComplexUnitType.Dip, currentQuestion.FontSize);
            }
        }

        public static void PrepareWhiteCard(View view, WhiteCard card)
        {
            var text = view.FindViewById<TextView>(Resource.Id.wc_CardText);
            if (AppFont != null)
            {
                var txtlogo = view.FindViewById<TextView>(Resource.Id.wc_logo_text);
                txtlogo.SetTypeface(AppFont, TypefaceStyle.Normal);
                text.SetTypeface(AppFont, TypefaceStyle.Normal);
            }

            if (card != null)
            {
                text.TextFormatted = Html.FromHtml(card.Text);
                text.SetTextSize(ComplexUnitType.Dip, card.FontSize);
            }
        }
    }
}