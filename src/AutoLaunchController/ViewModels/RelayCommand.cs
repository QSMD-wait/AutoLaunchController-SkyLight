using System;
using System.Windows.Input;

namespace AutoLaunchController.ViewModels
{
    /// <summary>
    /// 一个可绑定到 UI 控件的命令，它将命令的执行和可执行状态委托给其他对象。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        /// <summary>
        /// 初始化 <see cref="RelayCommand"/> 类的新实例。
        /// </summary>
        /// <param name="execute">命令执行的逻辑。</param>
        public RelayCommand(Action<object?> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// 初始化 <see cref="RelayCommand"/> 类的新实例。
        /// </summary>
        /// <param name="execute">命令执行的逻辑。</param>
        /// <param name="canExecute">确定命令是否可以执行的逻辑。</param>
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// 当影响命令是否应执行的更改发生时发生。
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 定义用于确定此命令是否可以在其当前状态下执行的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。如果命令不需要传递数据，则可以将此对象设置为 null。</param>
        /// <returns>如果可以执行此命令，则为 true；否则为 false。</returns>
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// 定义在调用此命令时要调用的方法。
        /// </summary>
        /// <param name="parameter">此命令使用的数据。如果命令不需要传递数据，则可以将此对象设置为 null。</param>
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}