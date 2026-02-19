# 可配置单例与MVVM弹窗服务实现方案

**版本**: 1.0
**作者**: 霂晓 (AI Assistant)
**日期**: 2026-02-19

---

## 1. 概述与设计哲学

本方案旨在为 `AutoLaunchController-SkyLight` 项目实现两大核心功能：
1.  **可配置的应用程序单例（互斥）运行机制**：允许通过配置文件控制程序是否多开，以及在多开时的具体行为。
2.  **严格遵循MVVM模式的对话框（弹窗）服务**：确保ViewModel在需要与用户进行弹窗交互时，不与任何UI框架产生耦合。

设计遵循以下核心原则：
- **配置驱动 (Configuration-Driven)**: 核心行为由外部 `settings.json` 文件定义。
- **关注点分离 (Separation of Concerns)**: 配置、逻辑、UI各司其职，互不干扰。
- **依赖倒置 (Dependency Inversion)**: ViewModel等上层模块依赖于抽象接口，而非具体实现。
- **可扩展性 (Extensibility)**: 整体架构易于未来增加新的互斥策略或弹窗类型。

---

## 2. Part 1: 可配置的单例运行机制

### 2.1. 目标

实现一个健壮的单例运行控制器，允许用户在配置文件中选择以下三种策略之一：
- `AllowMultiple`: 允许多个程序实例同时运行。
- `ShowMessageAndExit`: 只允许一个实例，新实例启动时会弹窗提示用户，然后自动退出。
- `SilentExit`: 只允许一个实例，新实例启动时会静默退出，无任何提示。

### 2.2. 配置文件设计 (`settings.json`)

将在 `settings.json` 中新增 `ApplicationBehavior` 层级，用于管理应用行为相关的配置。

```json
{
  "Logging": {
    // ... 日志配置
  },
  "ApplicationBehavior": {
    "SingleInstance": {
      "Strategy": "ShowMessageAndExit", // 可选: "AllowMultiple", "ShowMessageAndExit", "SilentExit"
      "MessageConfiguration": { // 为 ShowMessageAndExit 策略预留的详细配置
        "Style": "MessageBox",
        "AutoClose": false,
        "DurationMilliseconds": 3000,
        "EnsureSingleWindow": true
      }
    }
  }
}
```

### 2.3. 文件结构与职责

| 文件路径 (src/) | 类型 | 核心职责 |
| :--- | :--- | :--- |
| `AutoLaunchController.Infrastructure/Configuration/Models/AppSettings.cs` | **修改** | 定义与 `settings.json` 对应的C#模型，新增 `SingleInstanceSettings` 类和 `SingleInstanceStrategy` 枚举。 |
| `AutoLaunchController.Infrastructure/Application/MutexManager.cs` | **新建** | 封装 `System.Threading.Mutex` 的所有底层操作，提供 `TryAcquire` 和 `Release` 等原子化接口。 |
| `AutoLaunchController/App.xaml.cs` | **修改** | 作为总指挥，在 `OnStartup` 时加载配置，调用 `MutexManager` 执行策略，并决定程序是继续运行还是退出。在 `OnExit` 时释放锁。 |

### 2.4. 执行流程

1.  **App启动** -> `App.OnStartup` 被调用。
2.  **加载配置** -> 调用 `ConfigManager.LoadAppSettings()` 读取 `settings.json`。
3.  **获取策略** -> 从配置中解析出 `SingleInstance.Strategy`。
4.  **请求锁** -> 调用 `MutexManager.TryAcquire(strategy)`。
5.  **逻辑判断**:
    - `MutexManager` 内部根据策略决定是否创建 `Mutex`。
    - 如果是第一个实例，获取锁成功，返回 `true`。
    - 如果是重复实例，根据策略执行相应操作（准备弹窗或静默），返回 `false`。
6.  **程序决策**:
    - `OnStartup` 收到 `true` -> 继续加载主窗口。
    - `OnStartup` 收到 `false` -> 调用 `Application.Current.Shutdown()` 退出新实例。
7.  **App关闭** -> `App.OnExit` 调用 `MutexManager.Release()` 释放锁。

---

## 3. Part 2: MVVM弹窗服务

### 3.1. 目标

创建一个服务，使得ViewModel可以在不直接引用任何UI组件（如 `Window`, `MessageBox`）的情况下，请求显示一个对话框。这对于保持ViewModel的纯洁性和可测试性至关重要。

**核心比喻**: ViewModel是餐厅的“后厨大总管”，View是“前厅服务员”。大总管通过“内部对讲机” (`IDialogService`) 向服务员下达指令，但从不亲自去前厅。

### 3.2. 文件结构与职责

| 文件路径 (src/) | 类型 | 核心职责 |
| :--- | :--- | :--- |
| `AutoLaunchController.Core/Services/IDialogService.cs` | **新建** | **“对讲机设计图”**。定义了弹窗服务的抽象接口，是ViewModel的唯一依赖。 |
| `AutoLaunchController/Services/DialogService.cs` | **新建** | **“真正的对讲机”**。`IDialogService` 的WPF实现，位于UI项目中，负责调用 `MessageBox.Show()` 等具体UI操作。 |
| `AutoLaunchController/App.xaml.cs` | **修改** | **“安装工人”**。在DI容器中注册 `DialogService` 为 `IDialogService` 的实现。 |
| `*ViewModel.cs` (任意ViewModel) | **修改** | **“对讲机用户”**。通过构造函数注入 `IDialogService`，并在需要时调用其方法来请求弹窗。 |

### 3.3. 依赖关系与流程

**依赖关系**:
`ViewModel` -> `IDialogService` (接口) <- `DialogService` (实现)

**执行流程**:
1.  **用户操作** -> `ViewModel` 中的 `Command` 被触发。
2.  **请求弹窗** -> `ViewModel` 调用 `_dialogService.ShowMessageBox(...)` 方法。它只与接口交互。
3.  **服务执行** -> DI容器将调用转发给已注册的 `DialogService` 实例。
4.  **UI呈现** -> `DialogService` 在UI线程上调用 `MessageBox.Show()`，向用户显示弹窗。

---

## 4. 整合与总结

这两个功能将协同工作。当 `MutexManager` 检测到重复实例并需要弹窗时，它不会自己弹，而是通过 `IDialogService` 来请求弹窗（这需要将 `IDialogService` 注入到 `MutexManager` 或由 `App.xaml.cs` 协调）。

通过本方案，我们将构建一个配置灵活、架构清晰、高度解耦且易于测试和扩展的应用程序基础。