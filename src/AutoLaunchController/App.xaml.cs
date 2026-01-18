using System.Windows;

namespace AutoLaunchController;

/// <summary>
/// 提供应用程序特定的行为，并作为应用程序的入口点。
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 引发 <see cref="Application.Startup"/> 事件。
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="StartupEventArgs"/>。</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        var mainWindow = new MainWindow();
        // 将主窗口的数据上下文设置为 MainViewModel 的一个新实例。
        // 这是连接视图(MainWindow)和视图模型(MainViewModel)的关键步骤，
        // 使得XAML中的数据绑定能够正常工作。
        mainWindow.DataContext = new ViewModels.MainViewModel();
        mainWindow.Show();
        base.OnStartup(e);
    }
}