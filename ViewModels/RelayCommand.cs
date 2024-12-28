using System;
using System.Windows.Input;

namespace SmoothVideoPlayer.ViewModels
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> execute;
        readonly Func<object, bool> canExecute;
        public event EventHandler CanExecuteChanged;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (canExecute == null) return true;
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
} 