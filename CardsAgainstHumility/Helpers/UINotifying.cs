using System.ComponentModel;

namespace CardsAgainstHumility.Helpers
{
    public abstract class UINotifying : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}