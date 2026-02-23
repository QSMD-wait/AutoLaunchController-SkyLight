using System.Windows;
using AutoLaunchController.Core.Services;
using iNKORE.UI.WPF.Modern.Controls;

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
    /// 使用 iNKORE 的 <see cref="ModernMessageBox"/> 来显示一个现代风格的消息框。
    /// </summary>
    /// <param name="title">窗口标题。</param>
    /// <param name="message">显示的消息。</param>
    public void ShowMessageBox(string title, string message)
    {
        // Application.Current.Dispatcher.Invoke确保了即使调用来自非UI线程，弹窗也能在正确的线程上显示。
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 直接使用 iNKORE.UI.WPF.Modern.Controls.MessageBox 的静态 Show 方法。
            // 这样更简洁，也符合库的设计。
            // 为了避免与 System.Windows.MessageBox 冲突，这里使用了完整的命名空间。
            // 根据编译错误提示，最匹配的重载签名是 (string message, string title, MessageBoxButton button, MessageBoxImage icon)。
            // 我们严格按照此签名来调用。
            iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        });
    }
}