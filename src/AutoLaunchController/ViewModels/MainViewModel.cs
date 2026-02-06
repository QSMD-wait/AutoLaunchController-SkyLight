using AutoLaunchController.Services;

namespace AutoLaunchController.ViewModels;

/// <summary>
/// 代表主窗口的视图模型。
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly ILoggingService _logger;

    /// <summary>
    /// 获取主窗口的标题。
    /// </summary>
    public string Title { get; } = "AutoLaunchController";

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    public MainViewModel() : this(new LoggingService()) { }

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="loggingService">日志服务</param>
    public MainViewModel(ILoggingService loggingService)
    {
        _logger = loggingService;
        _logger.LogInformation("MainViewModel 初始化完成，标题: {Title}", Title);
    }
}