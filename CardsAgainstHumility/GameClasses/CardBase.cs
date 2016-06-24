using CardsAgainstHumility.Helpers;

namespace CardsAgainstHumility.GameClasses
{
    public abstract class CardBase : UINotifying
    {
        private string _text;
        public string Text
        {
            get
            {
                
                return (_text == null) ? string.Empty : _text.Replace("_", "_______");
            }
            set
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

        protected int _fontSize = 20;
        public int FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                _fontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        public CardBase(string text, int fontsize)
        {
            Text = text;
            FontSize = fontsize;
        }

        public CardBase(string text)
        {
            Text = text;
        }
    }
}