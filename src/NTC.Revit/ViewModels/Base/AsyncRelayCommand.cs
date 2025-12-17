using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NTC.Revit.ViewModels.Base
{
    public class AsyncRelayCommand : ICommand, INotifyPropertyChanged
    {
        private readonly Func<Task> _execute;
        private readonly Predicate<object> _canExecute;
        private readonly Action<Exception> _onException;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<Task> execute, Action<Exception> onException = null, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _onException = onException;
            _canExecute = canExecute;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            if (IsExecuting) return;

            IsExecuting = true;
            try
            {
                await _execute();
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
            finally
            {
                IsExecuting = false;
            }
        }
    }

    public class AsyncRelayCommand<T> : ICommand, INotifyPropertyChanged
    {
        private readonly Func<T, Task> _execute;
        private readonly Predicate<object> _canExecute;
        private readonly Action<Exception> _onException;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<T, Task> execute, Action<Exception> onException = null, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _onException = onException;
            _canExecute = canExecute;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool IsExecuting
        {
            get => _isExecuting;
            private set
            {
                if (_isExecuting != value)
                {
                    _isExecuting = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsExecuting)));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            if (IsExecuting) return;

            IsExecuting = true;
            try
            {
                if (parameter is T tParam)
                    await _execute(tParam);
                else
                    await _execute(default);
            }
            catch (Exception ex)
            {
                _onException?.Invoke(ex);
            }
            finally
            {
                IsExecuting = false;
            }
        }
    }
}
