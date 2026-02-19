using System;
using System.Threading;
using AutoLaunchController.Infrastructure.Configuration.Models;
using Serilog;

namespace AutoLaunchController.Infrastructure.Application;

/// <summary>
/// 提供应用程序单例（互斥）运行的底层实现。
/// 这是一个静态帮助类，封装了与 <see cref="System.Threading.Mutex"/> 相关的所有操作。
/// </summary>
/// <remarks>
/// <para>
///     [使用场景]：在应用程序启动的最早期（如 `App.OnStartup`），用于检查是否已有实例在运行，并根据配置策略执行相应操作。
/// </para>
/// <para>
///     [工作原理]：它通过创建一个系统级的、有唯一名称的 <see cref="Mutex"/> 来实现跨进程的实例检测。
///     `TryAcquire` 方法尝试获取这个锁。如果成功，说明是第一个实例；如果失败，说明已有实例存在。
/// </para>
/// <para>
///     [最佳实践]：此类应保持无状态和静态，其行为完全由传入的参数决定。
///     互斥锁的名称应足够独特，以避免与其他应用程序冲突。在程序退出时，必须调用 `Release` 方法以确保锁被正确释放。
/// </para>
/// </remarks>
public static class MutexManager
{
    /// <summary>
    /// 定义一个全局唯一的互斥锁名称。
    /// "Global\\" 前缀确保了互斥锁在所有用户会话中都是唯一的（即使在终端服务环境下）。
    /// </summary>
    private const string MutexName = "Global\\AutoLaunchController_SKYLIGHT_INSTANCE_MUTEX";

    private static Mutex? _mutex;
    
    /// <summary>
    /// 绑定到当前类的日志记录器。
    /// </summary>
    private static readonly ILogger _logger = Log.ForContext(typeof(MutexManager));

    /// <summary>
    /// 尝试根据指定的策略获取单例运行的权限。
    /// </summary>
    /// <param name="settings">应用程序的单例配置。</param>
    /// <param name="onShowMessage">当策略为 <see cref="SingleInstanceStrategy.ShowMessageAndExit"/> 且实例已存在时，将执行此回调操作来显示消息。</param>
    /// <returns>如果应用程序应该继续运行，则返回 <c>true</c>；如果应立即退出，则返回 <c>false</c>。</returns>
    public static bool TryAcquire(SingleInstanceSettings settings, Action onShowMessage)
    {
        _logger.Information("正在执行单例检查，策略为: {Strategy}", settings.Strategy);
        
        if (settings.Strategy == SingleInstanceStrategy.AllowMultiple)
        {
            _logger.Information("配置允许多个实例运行，跳过互斥锁检查。");
            return true; // 策略允许多开，直接放行
        }

        _logger.Debug("尝试创建或打开名为 '{MutexName}' 的系统互斥锁。", MutexName);
        _mutex = new Mutex(true, MutexName, out bool createdNew);

        if (createdNew)
        {
            _logger.Information("成功创建新的互斥锁。此为应用程序的唯一实例。");
            return true; // 成功创建了新的互斥锁，是第一个实例，放行
        }

        // --- 如果执行到这里，说明互斥锁已存在，当前是重复实例 ---
        _logger.Warning("检测到名为 '{MutexName}' 的互斥锁已存在，表明已有实例在运行。", MutexName);
        switch (settings.Strategy)
        {
            case SingleInstanceStrategy.ShowMessageAndExit:
                // 策略要求弹窗提示，执行传入的回调
                _logger.Debug("根据策略，准备执行显示消息的回调。");
                onShowMessage?.Invoke();
                break;
            case SingleInstanceStrategy.SilentExit:
                // 策略要求静默退出，什么都不做
                _logger.Debug("根据策略，将执行静默退出。");
                break;
        }

        return false; // 告知调用方，应立即退出
    }

    /// <summary>
    /// 释放持有的互斥锁。
    /// 应在应用程序退出时（如 `App.OnExit`）调用。
    /// </summary>
    public static void Release()
    {
        if (_mutex == null)
        {
            _logger.Debug("当前没有持有的互斥锁，无需释放。");
            return;
        }
        
        _logger.Information("正在释放应用程序实例互斥锁...");
        try
        {
            _mutex.ReleaseMutex();
            _mutex.Dispose();
            _mutex = null;
            _logger.Information("互斥锁已成功释放。");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "释放互斥锁时发生意外错误。");
        }
    }
}