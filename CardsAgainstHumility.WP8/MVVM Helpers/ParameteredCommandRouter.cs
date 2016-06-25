using System;
using System.Windows.Input;

namespace CardsAgainstHumility.WP8.MVVM_Helpers
{
    class ParameteredCommandRouter<T> : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Func<T, bool> _canExecute;
        Action<T> _execute;

        public ParameteredCommandRouter(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentException("execute cannot be null");
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute.Invoke((T)parameter);
            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke((T)parameter);
        }
    }
}
