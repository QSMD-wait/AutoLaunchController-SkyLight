# AutoLaunchController 单例服务指南 (v2.0)

## 1. 概述

本文档为 `AutoLaunchController` 项目中**可配置单例服务**的实现指南。该服务通过系统互斥锁（Mutex）确保在任何时候只有一个应用实例在运行，并提供灵活的冲突处理策略。

> **注意**: 关于弹窗服务的详细指南，请参阅 [./DialogServiceGuide.md](./DialogServiceGuide.md)。

## 2. 可配置单例服务

该服务由 `MutexManager` 静态类提供，旨在通过简单、统一的接口管理应用的单例行为。

### 2.1. 核心实现 `MutexManager`

-   **工作原理**：在应用启动时，`MutexManager.TryAcquire()` 方法会尝试获取一个全局唯一的系统互斥锁。
    -   如果成功获取，则允许应用继续运行。
    -   如果获取失败（意味着已有实例在运行），则根据配置文件中的策略执行相应操作。
-   **代码示例** (`src/AutoLaunchController.Infrastructure/Application/MutexManager.cs`):
    ```csharp
    public static class MutexManager
    {
        private static Mutex _mutex;
        private static readonly ILogger _logger = Log.ForContext(typeof(MutexManager));

        public static bool TryAcquire(SingleInstanceSettings settings, Action onShowMessage)
        {
            // ... (实现细节) ...
            if (createdNew)
            {
                return true;
            }
            else
            {
                if (settings.Strategy == SingleInstanceStrategy.ShowMessageAndExit)
                {
                    onShowMessage?.Invoke();
                }
                return false; // 指示应用应退出
            }
        }
    }
    ```

### 2.2. 配置模型与集成

-   **配置** (`src/AutoLaunchController.Infrastructure/Configuration/Models/AppSettings.cs`):
    通过 `SingleInstanceSettings` 类和 `SingleInstanceStrategy` 枚举来定义单例策略。
    ```csharp
    public class SingleInstanceSettings
    {
        public SingleInstanceStrategy Strategy { get; set; } = SingleInstanceStrategy.ShowMessageAndExit;
        public string MutexName { get; set; } = "AutoLaunchController_GlobalMutex";
    }

    public enum SingleInstanceStrategy
    {
        NoMutex,
        ShowMessageAndExit,
        SilentExit
    }
    ```
-   **集成** (`src/AutoLaunchController/App.xaml.cs`):
    在 `OnStartup` 方法中，加载配置并调用 `MutexManager`。当需要通知用户时，`MutexManager` 会调用一个委托，该委托内部使用了 `IDialogService`。
    ```csharp
    protected override void OnStartup(StartupEventArgs e)
    {
        // ...
        bool shouldContinue = MutexManager.TryAcquire(
            appSettings.SingleInstance,
            () => _ = _dialogService?.ShowAsync("提示", "程序已经在运行了喵~")
        );

        if (!shouldContinue)
        {
            Shutdown();
            return;
        }
        // ...
    }
    ```

## 3. 核心模块索引

| 文件路径                                                                    | 职责说明                                                                                             |
| --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `src/AutoLaunchController.Infrastructure/Application/MutexManager.cs`       | **单例服务核心**。负责在应用启动时检查并获取系统互斥锁。                                           |
| `src/AutoLaunchController.Infrastructure/Configuration/Models/AppSettings.cs` | **配置模型**。定义了包括单例策略在内的所有应用配置项。                                               |
| `src/AutoLaunchController/App.xaml.cs`                                      | **应用入口**。负责在启动时集成并调用 `MutexManager`。                                                |