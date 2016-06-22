namespace CardsAgainstHumility.GameClasses
{
    public class WhiteCard : CardBase
    {
        public WhiteCard(string text, int fontsize) : base(text, fontsize)
        {
            IsSelected = false;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
    }
}