using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AutoLaunchController.ViewModels;

/// <summary>
/// 为视图模型提供属性更改通知的基类，实现了 INotifyPropertyChanged 接口。
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    /// <summary>
    /// 当属性值更改时发生。
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 设置属性值，如果值已更改，则会引发 PropertyChanged 事件。
    /// </summary>
    /// <typeparam name="T">属性的类型。</typeparam>
    /// <param name="field">要设置的后备字段。</param>
    /// <param name="value">属性的新值。</param>
    /// <param name="propertyName">属性的名称。这是可选的，并且可以由调用方自动提供。</param>
    /// <returns>如果值已更改并引发了事件，则为 true；否则为 false。</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// 使用提供的属性名称引发 PropertyChanged 事件。
    /// </summary>
    /// <param name="propertyName">已更改的属性的名称。这是可选的，并且可以由调用方自动提供。</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}