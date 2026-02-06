using System.Windows;
using Serilog;
using Serilog.Sinks.File;
using System.Text;
using System;
using AutoLaunchController.Services;

namespace AutoLaunchController;

/// <summary>
/// 提供应用程序特定的行为，并作为应用程序的入口点。
/// </summary>
public partial class App : Application
{
    private ILoggingService? _loggingService;

    /// <summary>
    /// 引发 <see cref="Application.Startup"/> 事件。
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="StartupEventArgs"/>。</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // 设置控制台输出编码为UTF-8，防止中文乱码
        Console.OutputEncoding = Encoding.UTF8;
        
        // 配置Serilog
        ConfigureLogging();

        // 创建日志服务实例
        _loggingService = new LoggingService();

        // 记录应用程序启动
        _loggingService.LogInformation("应用程序启动");

        var mainWindow = new MainWindow();
        // 将主窗口的数据上下文设置为 MainViewModel 的一个新实例。
        // 这是连接视图(MainWindow)和视图模型(MainViewModel)的关键步骤，
        // 使得XAML中的数据绑定能够正常工作。
        mainWindow.DataContext = new ViewModels.MainViewModel(_loggingService);
        mainWindow.Show();
        base.OnStartup(e);
    }

    /// <summary>
    /// 配置Serilog日志记录
    /// </summary>
    private static void ConfigureLogging()
    {
        // 配置Serilog记录器，显式指定文件输出编码为UTF-8
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs\\application-.log", 
                rollingInterval: RollingInterval.Day, 
                retainedFileCountLimit: 7,
                encoding: Encoding.UTF8)
            .CreateLogger();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _loggingService?.LogInformation("应用程序退出");
        Log.CloseAndFlush(); // 确保所有日志都写入
        base.OnExit(e);
    }
}