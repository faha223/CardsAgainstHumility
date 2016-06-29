using Android.Widget;

namespace CardsAgainstHumility.Android.Controls
{
    public class SelectableItem
    {
        public bool IsSelected { get; set; }
        public string Text { get; set; }

        public CheckBox View { get; set; }

        public int Index { get; set; }
    }
}