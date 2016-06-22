namespace CardsAgainstHumility.GameClasses
{
    public class BlackCard : CardBase
    {
        public BlackCard(string text, int fontsize, int pick, int draw) : base(text, fontsize)
        {
            Pick = (byte)pick;
            Draw = (byte)draw;
        }

        public BlackCard(string text, int fontsize, int pick) : base(text, fontsize)
        {
            Pick = (byte)pick;
        }

        private byte _pick;
        public int Pick
        {
            get
            {
                return _pick;
            }
            set
            {
                _pick = (byte)value;
                OnPropertyChanged("Pick");
            }
        }

        private byte _draw = 0;
        public int Draw
        {
            get
            {
                return _draw;
            }
            set
            {
                _draw = (byte)(value.HasValue ? value.Value : 0);
                OnPropertyChanged("Draw");
            }
        }
    }
}