using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility.GameClasses
{
    public abstract class CardBase : UINotifying
    { 
        private string _text { get; set; }
        public string Text
        {
            get
            {
                
                return (_text == null) ? string.Empty : _text.Replace("_", "_______");
            }
            protected set
            {
                _text = value;
            }
        }
        public string Id
        {
            get
            {
                return _text;
            }
        }

        public int FontSize { get; protected set; }

        public CardBase(string text, int fontsize)
        {
            Text = text;
            FontSize = fontsize;
        }
    }
}