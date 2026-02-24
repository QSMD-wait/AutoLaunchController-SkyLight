using System.Threading.Tasks;
using AutoLaunchController.Core.Services.Dialogs;

namespace AutoLaunchController.Core.Services;

/// <summary>
/// 定义了用于显示对话框的应用服务接口，以解耦ViewModel与View。
/// </summary>
/// <remarks>
/// <para>
///     [使用场景]：当ViewModel需要与用户进行弹窗交互（如显示通知、请求确认）时，应通过此接口进行。
/// </para>
/// <para>
///     [工作原理]：这是一个抽象契约（如同“对讲机设计图”）。ViewModel仅依赖此接口，
///     而具体的UI弹窗逻辑（如调用WPF的MessageBox）则由在UI层实现的具体类（“真正的对讲机”）完成。
///     通过依赖注入，ViewModel在运行时会获得具体的实现实例。
/// </para>
/// <para>
///     [最佳实践]：保持此接口的通用性，不应包含任何特定于UI框架的类型。
///     方法应定义清晰的意图，如“显示消息”、“请求确认”等。
/// </para>
/// </remarks>
public interface IDialogService
{
    /// <summary>
    /// 使用参数对象显示一个功能丰富的消息框，并异步等待结果。
    /// </summary>
    /// <param name="parameters">封装了所有弹窗配置的参数对象。</param>
    /// <returns>一个表示用户选择结果的任务。</returns>
    Task<DialogResult> ShowAsync(MessageBoxParameters parameters);

    /// <summary>
    /// (便捷重载) 显示一个简单的通知消息框。
    /// </summary>
    /// <param name="title">弹窗标题。</param>
    /// <param name="message">弹窗消息。</param>
    /// <param name="icon">弹窗图标，默认为信息图标。</param>
    Task ShowAsync(string title, string message, DialogIcon icon = DialogIcon.Information);
}