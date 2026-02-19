using AutoLaunchController.Core.Services;
using AutoLaunchController.Services;
using Serilog;
using System;
using System.Windows.Input;

namespace AutoLaunchController.ViewModels;

/// <summary>
/// 代表主窗口的视图模型。
/// </summary>
/// <remarks>
/// <para>
///     [使用场景]：作为主窗口（MainWindow）的数据上下文，负责处理用户交互逻辑和业务数据，实现视图与模型的分离。
/// </para>
/// <para>
///     [工作原理]：您可以把这个类想象成“主窗口的大脑”。它接收来自视图（XAML）的用户操作（如按钮点击），然后执行相应的业务逻辑（如记录日志），并通过数据绑定自动更新视图的显示。
/// </para>
/// <para>
///     [最佳实践]：建议通过构造函数注入依赖项（如日志服务），以提高代码的可测试性和可维护性。视图模型应专注于业务逻辑，避免直接操作UI元素。
/// </para>
/// </remarks>
public class MainViewModel : BaseViewModel
{
    private readonly ILogger _logger;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// 获取主窗口的标题。
    /// </summary>
    public string Title { get; } = "AutoLaunchController";

    /// <summary>
    /// 获取用于测试日志输出的命令。
    /// </summary>
    public ICommand TestLogCommand { get; }

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    public MainViewModel() : this(new LoggingService(), new DialogService()) { }

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="loggingServiceFactory">日志服务工厂。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <remarks>
    /// <para>
    ///     [使用场景]：在需要自定义日志服务或进行单元测试时使用，支持依赖注入。
    /// </para>
    /// <para>
    ///     [工作原理]：这个构造函数接收一个日志服务工厂，用于为MainViewModel创建专属的日志记录器，确保日志消息能够准确标识来源。
    /// </para>
    /// <para>
    ///     [最佳实践]：这是推荐使用的主构造函数，它明确声明了视图模型的依赖关系，使得代码更容易测试和维护。
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">当 <paramref name="loggingServiceFactory"/> 为 null 时抛出。</exception>
    public MainViewModel(ILoggingService loggingServiceFactory, IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        // 通过工厂为 MainViewModel 创建专属的日志记录器
        _logger = loggingServiceFactory.ForContext<MainViewModel>() ?? throw new ArgumentNullException(nameof(loggingServiceFactory));
        _logger.Information("MainViewModel 初始化完成，标题: {Title}", Title);

        // 初始化命令
        TestLogCommand = new RelayCommand(TestDialogOutput);
    }

    private void TestDialogOutput(object? parameter)
    {
        _logger.Information("准备通过 IDialogService 显示测试弹窗...");
        _dialogService.ShowMessageBox("测试弹窗", "这是一个通过MVVM模式弹出的窗口喵！");
        _logger.Information("测试弹窗显示完毕。");
    }
}