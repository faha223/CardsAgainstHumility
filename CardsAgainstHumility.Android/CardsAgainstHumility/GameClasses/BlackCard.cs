namespace CardsAgainstHumility.GameClasses
{
    public class BlackCard : CardBase
    {
        public BlackCard(string text, int fontsize, int pick, int draw) : base(text, fontsize)
        {
            Pick = (byte)pick;
            Draw = (byte)draw;
        }

        private byte _pick { get; set; }

        private byte _draw { get; set; }

        public int Pick
        {
            get
            {
                return _pick;
            }
            set
            {
                _pick = (byte)value;
                NotifyPropertyChanged("Pick");
            }
        }

        public int? Draw
        {
            get
            {
                return _draw;
            }
            set
            {
                _draw = (byte)value;
                NotifyPropertyChanged("Draw");
            }
        }
    }
}