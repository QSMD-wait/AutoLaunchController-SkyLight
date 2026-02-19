using System.Windows;
using AutoLaunchController.Core.Services;

namespace AutoLaunchController.Services;

/// <summary>
/// <see cref="IDialogService"/> 接口的WPF平台具体实现。
/// </summary>
/// <remarks>
/// <para>
///     [使用场景]：在WPF应用程序的UI层，作为 <see cref="IDialogService"/> 接口的具体提供者。
/// </para>
/// <para>
///     [工作原理]：您可以把这个类想象成“连接着真实酒柜的对讲机”。当ViewModel通过接口（“对讲机按钮”）请求弹窗时，
///     这个类会执行具体的WPF弹窗操作（“去酒柜拿酒并展示给客人”）。
/// </para>
/// <para>
///     [最佳实践]：此类应被注册为 <see cref="IDialogService"/> 的单例实现。
///     所有与WPF窗口系统相关的弹窗逻辑都应封装在此类中。
/// </para>
/// </remarks>
public class DialogService : IDialogService
{
    /// <summary>
    /// 使用WPF的 <see cref="MessageBox"/> 来显示一个标准的消息框。
    /// </summary>
    /// <param name="title">窗口标题。</param>
    /// <param name="message">显示的消息。</param>
    public void ShowMessageBox(string title, string message)
    {
        // 由于此类位于UI项目中，因此可以安全地访问和使用WPF的UI组件。
        // Application.Current.Dispatcher.Invoke确保了即使调用来自非UI线程，弹窗也能在正确的线程上显示。
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        });
    }
}