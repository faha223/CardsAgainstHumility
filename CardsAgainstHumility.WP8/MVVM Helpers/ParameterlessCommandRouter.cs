using System;
using System.Windows.Input;

namespace CardsAgainstHumility.WP8.MVVM_Helpers
{
    class ParameterlessCommandRouter : ICommand
    {
        public event EventHandler CanExecuteChanged;

        Func<bool> _canExecute;
        Action _execute;

        public ParameterlessCommandRouter(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentException("execute cannot be null");
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute != null)
                return _canExecute.Invoke();
            return true;
        }

        public void Execute(object parameter)
        {
            _execute.Invoke();
        }
    }
}
