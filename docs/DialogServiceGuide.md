# AutoLaunchController 弹窗服务指南 (v1.0)

## 1. 概述

本文档为 `AutoLaunchController` 项目中基于 **MVVM** 设计模式的弹窗服务 (`IDialogService`) 的实现与使用指南。该服务旨在将 ViewModel 与任何具体的 UI 框架解耦，提高代码的可测试性、可维护性和灵活性。

系统使用了 `iNKORE.UI.WPF.Modern` 库来提供具有现代风格的 UI 弹窗。

## 2. 设计哲学与工作原理

### 2.1. 为什么需要 `IDialogService`？

在 MVVM 模式中，ViewModel 层不应该直接引用或操作 View（UI 元素）。如果 ViewModel 直接调用 `System.Windows.MessageBox.Show()`，就会产生对 `PresentationFramework.dll` (WPF) 的硬依赖，这将导致：
-   **可测试性差**：单元测试 ViewModel 时，会弹出真实的 UI 窗口，无法进行自动化测试。
-   **灵活性低**：如果未来想更换 UI 框架（例如从 WPF 迁移到 WinUI），所有 ViewModel 都需要重写。
-   **职责不清**：ViewModel 的职责是处理业务逻辑和状态，而不是关心弹窗长什么样。

### 2.2. 工作原理：依赖倒置

`IDialogService` 应用了**依赖倒置原则**：
1.  **定义抽象**：在最核心的 `AutoLaunchController.Core` 项目中，我们定义一个与 UI 无关的 `IDialogService` 接口。
2.  **ViewModel 依赖抽象**：ViewModel 仅持有 `IDialogService` 接口的引用，通过它请求弹窗。
3.  **UI 层实现抽象**：在 `AutoLaunchController` (WPF) 项目中，我们创建一个 `DialogService` 类来实现这个接口，该类内部会调用 `iNKORE` 的弹窗。
4.  **依赖注入**：在应用启动时，将 `DialogService` 的实例注入到需要它的 ViewModel 中。

这样，ViewModel 就从“我需要一个 WPF 弹窗”变成了“我需要一个能对话的东西”，具体是什么“东西”由上层（UI层）决定。

## 3. 核心组件

### 3.1. `IDialogService` 接口

这是弹窗服务的核心契约，定义了可以执行的操作。

-   **路径**: `src/AutoLaunchController.Core/Services/IDialogService.cs`
-   **定义**:
    ```csharp
    public interface IDialogService
    {
        // 显示一个功能丰富的消息框，并异步等待结果
        Task<DialogResult> ShowAsync(MessageBoxParameters parameters);

        // (便捷重载) 显示一个简单的通知消息框
        Task ShowAsync(string title, string message, DialogIcon icon = DialogIcon.Information);
    }
    ```

### 3.2. `MessageBoxParameters` 参数对象

为了避免方法有过多参数，并提供优秀的扩展性，我们使用一个参数对象来封装所有弹窗配置。

-   **路径**: `src/AutoLaunchController.Core/Services/Dialogs/MessageBoxParameters.cs`
-   **定义**:
    ```csharp
    public class MessageBoxParameters
    {
        public string Title { get; }
        public string Message { get; }
        public DialogButtonSet Buttons { get; set; } = DialogButtonSet.OK;
        public DialogIcon Icon { get; set; } = DialogIcon.Information;
        public DialogDefaultButton DefaultButton { get; set; } = DialogDefaultButton.None;
        public DialogSound Sound { get; set; } = DialogSound.None; // 默认无声

        public MessageBoxParameters(string title, string message) { /* ... */ }
    }
    ```

### 3.3. 核心枚举

所有与弹窗相关的选项都被定义为与 UI 框架无关的枚举，存放在 `src/AutoLaunchController.Core/Services/Dialogs/` 目录下。
-   `DialogIcon`: 定义弹窗图标 (Information, Warning, Error, etc.)。
-   `DialogButtonSet`: 定义按钮组合 (OK, OKCancel, YesNo, etc.)。
-   `DialogResult`: 定义弹窗的返回结果 (OK, Cancel, Yes, No, etc.)。
-   `DialogDefaultButton`: 定义默认焦点按钮 (Primary, Secondary)。
-   `DialogSound`: 定义播放的声音 (None, Auto, Beep, etc.)。

## 4. 使用方法

在 ViewModel 中，通过构造函数注入 `IDialogService`，然后调用其方法。

### 示例1：显示一个简单的通知

使用便捷重载，代码非常清爽。

```csharp
public class MyViewModel : BaseViewModel
{
    private readonly IDialogService _dialogService;

    public MyViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public async Task SaveDataAsync()
    {
        // ... 保存数据 ...
        await _dialogService.ShowAsync("保存成功", "您的设置已成功保存！");
    }
}
```

### 示例2：请求用户确认

使用参数对象进行详细配置，并根据用户的选择执行不同逻辑。

```csharp
public async Task DeleteItemAsync(Item item)
{
    var parameters = new MessageBoxParameters("确认删除", $"您确定要删除项目 '{item.Name}' 吗？\n此操作不可恢复！")
    {
        Buttons = DialogButtonSet.YesNo,
        Icon = DialogIcon.Warning,
        DefaultButton = DialogDefaultButton.Secondary // 默认选中 "No"，防止误操作
    };

    var result = await _dialogService.ShowAsync(parameters);

    if (result == DialogResult.Yes)
    {
        // ... 执行删除逻辑 ...
        _logger.Information("用户确认删除项目 {ItemName}。", item.Name);
    }
    else
    {
        _logger.Information("用户取消了删除操作。");
    }
}
```

## 5. 扩展指南

该架构具有极佳的可扩展性。如果未来需要支持新的弹窗功能（例如，一个带输入框的弹窗）：
1.  **扩展参数**: 在 `MessageBoxParameters` 中添加新属性，如 `public string InputText { get; set; }`。
2.  **修改实现**: 在 `DialogService` (WPF实现) 中，检查 `parameters.InputText` 是否为空。如果不为空，则调用一个自定义的、带输入框的WPF窗口，而不是 `ModernMessageBox`。
3.  **接口不变**: `IDialogService` 接口和现有的 ViewModel 调用代码**完全无需改动**。

## 6. 核心模块索引

| 文件路径                                                                    | 职责说明                                                                                             |
| --------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `src/AutoLaunchController.Core/Services/IDialogService.cs`                  | **弹窗服务接口**。定义了弹窗功能的抽象契约，实现了与UI框架的解耦。                                       |
| `src/AutoLaunchController/Services/DialogService.cs`                        | **弹窗服务WPF实现**。`IDialogService` 的具体实现，使用 `iNKORE` 库来显示现代风格的弹窗。 |
| `src/AutoLaunchController.Core/Services/Dialogs/`                           | **核心模型目录**。存放所有与UI无关的弹窗参数对象 (`MessageBoxParameters`) 和枚举。                   |
| `src/AutoLaunchController/ViewModels/MainViewModel.cs`                      | **使用示例**。演示了如何在 ViewModel 中通过依赖注入使用 `IDialogService`。                             |
| `src/AutoLaunchController/App.xaml.cs`                                      | **集成与注入**。负责在应用启动时创建 `DialogService` 实例并注入到需要的模块中。                      |