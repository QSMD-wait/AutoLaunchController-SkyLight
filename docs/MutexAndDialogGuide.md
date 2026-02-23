# AutoLaunchController 单例与弹窗服务指南 (v1.0)

## 1. 概述

本文档为 `AutoLaunchController` 项目中两个关键UI相关服务的实现指南：
1.  **可配置单例服务**：通过系统互斥锁（Mutex）确保在任何时候只有一个应用实例在运行，并提供灵活的冲突处理策略。
2.  **MVVM弹窗服务**：构建了一个解耦的、可测试的弹窗服务，遵循MVVM设计模式，并使用 `iNKORE.UI.WPF.Modern` 库提供现代风格的UI。

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
            _logger.Information("正在执行单例检查，策略为: {Strategy}", settings.Strategy);

            if (settings.Strategy == SingleInstanceStrategy.NoMutex)
            {
                _logger.Information("配置为允许多实例运行，跳过互斥锁检查。");
                return true;
            }

            _mutex = new Mutex(true, settings.MutexName, out bool createdNew);

            if (createdNew)
            {
                _logger.Information("成功获取互斥锁，应用实例为首次创建。");
                return true;
            }
            else
            {
                _logger.Warning("互斥锁已被占用，检测到已有应用实例在运行。");
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
    在 `OnStartup` 方法中，加载配置并调用 `MutexManager`。
    ```csharp
    protected override void OnStartup(StartupEventArgs e)
    {
        // ...
        bool shouldContinue = MutexManager.TryAcquire(
            appSettings.SingleInstance,
            () => _dialogService.ShowMessageBox("程序已在运行", "AutoLaunchController 已经在后台运行了喵~")
        );

        if (!shouldContinue)
        {
            _logger.Warning("单例检查失败，根据配置策略，应用程序将退出。");
            Shutdown();
            return;
        }
        // ...
    }
    ```

## 3. MVVM 弹窗服务

该服务旨在将弹窗逻辑与ViewModel分离，提高代码的可测试性和可维护性。

### 3.1. 核心抽象 `IDialogService`

-   **定义** (`src/AutoLaunchController.Core/Services/IDialogService.cs`):
    在 `Core`（领域）层定义一个简单的接口，不依赖任何具体的UI框架。
    ```csharp
    public interface IDialogService
    {
        void ShowMessageBox(string title, string message);
    }
    ```

### 3.2. WPF 实现 `DialogService`

-   **实现** (`src/AutoLaunchController/Services/DialogService.cs`):
    在UI层提供接口的具体实现，负责调用 `iNKORE` 库来显示WPF弹窗。
-   **代码示例与经验分享**:
    ```csharp
    public class DialogService : IDialogService
    {
        public void ShowMessageBox(string title, string message)
        {
            // 保证UI操作在主线程上执行
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 经过一番“探险”，我们最终确定了 Show 方法的正确静态签名！
                // 它需要 (内容, 标题, 按钮枚举, 图标枚举) 这样的参数顺序。
                iNKORE.UI.WPF.Modern.Controls.MessageBox.Show(
                    message,
                    title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            });
        }
    }
    ```
    > **霂晓的小贴士**：当我们不确定一个库方法如何使用时，与其反复猜测，不如仔细观察编译器的错误提示。它们往往能告诉我们正确的参数类型和顺序，就像这次一样，喵~  
    > 正确的！！

## 4. 核心模块索引

| 文件路径                                                                    | 职责说明                                                                                             |
| --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `src/AutoLaunchController.Infrastructure/Application/MutexManager.cs`       | **单例服务核心**。负责在应用启动时检查并获取系统互斥锁。                                           |
| `src/AutoLaunchController.Core/Services/IDialogService.cs`                  | **弹窗服务接口**。定义了弹窗功能的抽象，实现了与UI框架的解耦。                                       |
| `src/AutoLaunchController/Services/DialogService.cs`                        | **弹窗服务实现**。`IDialogService` 的WPF平台具体实现，使用 `iNKORE` 库来显示现代风格的弹窗。 |
| `src/AutoLaunchController.Infrastructure/Configuration/Models/AppSettings.cs` | **配置模型**。定义了包括单例策略在内的所有应用配置项。                                               |
| `src/AutoLaunchController/App.xaml.cs`                                      | **应用入口**。负责在启动时集成并调用 `MutexManager` 和 `DialogService`。                             |