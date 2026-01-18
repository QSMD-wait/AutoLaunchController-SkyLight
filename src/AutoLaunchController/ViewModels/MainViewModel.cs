namespace AutoLaunchController.ViewModels;

/// <summary>
/// 代表主窗口的视图模型。
/// </summary>
public class MainViewModel : BaseViewModel
{
    /// <summary>
    /// 获取主窗口的标题。
    /// </summary>
    public string Title { get; } = "AutoLaunchController";

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    public MainViewModel()
    {
        System.Console.WriteLine("[DIAGNOSTIC] MainViewModel initialized with Title: " + Title);
    }
}