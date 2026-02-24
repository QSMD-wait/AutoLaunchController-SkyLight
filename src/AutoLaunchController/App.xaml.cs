using System.Windows;
using Serilog;
using System.Text;
using System;
using AutoLaunchController.Core.Services;
using AutoLaunchController.Infrastructure.Application;
using AutoLaunchController.Services;
using AutoLaunchController.Infrastructure.Bootstrap;
using AutoLaunchController.Infrastructure.Configuration;
using AutoLaunchController.Infrastructure.Logging;
using AutoLaunchController.Core.Services.Dialogs;

namespace AutoLaunchController;

/// <summary>
/// 提供应用程序特定的行为，并作为应用程序的入口点。
/// </summary>
/// <remarks>
/// <para>
///     [使用场景]：作为WPF应用程序的入口点，负责应用程序级别的初始化和资源管理。
/// </para>
/// <para>
///     [工作原理]：您可以把这个类想象成“应用程序的总指挥”。它在应用程序启动时协调各个核心模块的初始化顺序，在应用程序退出时确保资源被正确释放。
/// </para>
/// <para>
///     [最佳实践]：建议将应用程序级别的初始化逻辑（如目录创建、配置加载、日志系统初始化）放在此类中，以确保这些操作在应用程序生命周期的正确时间点执行。
/// </para>
/// </remarks>
public partial class App : Application
{
    private ILoggingService? _loggingService;
    private IDialogService? _dialogService;
    private ILogger? _logger;

    /// <summary>
    /// 引发 <see cref="Application.Startup"/> 事件。
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="StartupEventArgs"/>。</param>
    /// <remarks>
    /// <para>
    ///     [使用场景]：在应用程序启动时自动调用，是执行应用程序级别初始化操作的标准位置。
    /// </para>
    /// <para>
    ///     [工作原理]：这个方法按照固定的顺序执行初始化流程：1.设置控制台编码 → 2.初始化目录 → 3.加载配置 → 4.初始化日志 → 5.创建主窗口。这个顺序确保了后续操作都有正确的基础环境。
    /// </para>
    /// <para>
    ///     [最佳实践]：保持初始化流程的顺序性和幂等性。每个初始化步骤都应该是独立的，并且可以安全地重复执行而不会产生副作用。
    /// </para>
    /// </remarks>
    protected override async void OnStartup(StartupEventArgs e)
    {
        // 设置控制台输出编码为UTF-8，防止中文乱码
        Console.OutputEncoding = Encoding.UTF8;

        // --- 核心初始化流程 ---
        // 1. 初始化目录结构
        DirectoryManager.InitializeCoreDirectories();

        // 2. 加载或创建配置文件
        var appSettings = ConfigManager.LoadOrInitialize();

        // 3. 根据配置初始化日志系统
        LogManager.Initialize(appSettings);
        
        // 4. 创建核心服务实例
        _loggingService = new LoggingService();
        _dialogService = new DialogService();
        
        // 使用工厂为 App 类自身创建一个日志记录器
        _logger = _loggingService.ForContext<App>();

        // 5. 执行单例检查
        if (!MutexManager.TryAcquire(appSettings.SingleInstance))
        {
            // 如果是重复实例，根据策略决定是否弹窗
            if (appSettings.SingleInstance.Strategy == Infrastructure.Configuration.Models.SingleInstanceStrategy.ShowMessageAndExit)
            {
                // 使用 await 等待弹窗关闭，确保用户看到信息。
                // 这是 OnStartup 必须是 async void 的原因。
                var parameters = new MessageBoxParameters("提示", "程序已经在运行了喵~")
                {
                    Icon = DialogIcon.Information
                };
                
                if (_dialogService != null)
                {
                    await _dialogService.ShowAsync(parameters);
                }
            }
            
            // 此处记录的日志是 App 级别的决策，是正确的。
            _logger.Warning("单例检查失败，根据配置策略 ({Strategy})，应用程序将退出。", appSettings.SingleInstance.Strategy);
            Shutdown();
            return;
        }
        // --- 初始化完成 ---

        // 记录应用程序启动
        _logger.Information("应用程序启动，主窗口已创建并显示。");

        var mainWindow = new MainWindow();
        // 将主窗口的数据上下文设置为 MainViewModel 的一个新实例。
        // 这是连接视图(MainWindow)和视图模型(MainViewModel)的关键步骤，
        // 使得XAML中的数据绑定能够正常工作。 
        // 此处手动注入依赖
        mainWindow.DataContext = new ViewModels.MainViewModel(_loggingService, _dialogService);
        mainWindow.Show();
        base.OnStartup(e);
    }

    /// <summary>
    /// 引发 <see cref="Application.Exit"/> 事件。
    /// </summary>
    /// <param name="e">包含事件数据的 <see cref="ExitEventArgs"/>。</param>
    protected override void OnExit(ExitEventArgs e)
    {
        _logger?.Information("应用程序正在退出...");
        MutexManager.Release(); // 释放互斥锁
        LogManager.Shutdown(); // 使用我们封装的方法来关闭日志
        base.OnExit(e);
    }
}