# AutoLaunchController 日志系统使用指南

## 1. 概述

本文档旨在为开发者提供在 `AutoLaunchController` 项目中正确使用日志记录功能的标准流程。

本项目的日志系统基于 **Serilog** 实现，并采用 **工厂模式** 进行封装，以确保日志来源的上下文准确性。

## 2. 核心设计

系统的核心是 `ILoggingService` 接口。此接口不直接处理日志记录，而是作为一个 **日志记录器工厂**。

它的主要职责是：为应用程序中任何需要记录日志的类（如 ViewModel 或 Service），创建一个绑定了该类类型信息（SourceContext）的 `Serilog.ILogger` 实例。

这种设计确保了所有输出的日志都能准确地追溯到其来源类。

## 3. 使用方法

在任意类中实现日志记录，需遵循以下三个步骤。

以创建一个名为 `MyNewService` 的新服务为例：

### 步骤 1：依赖注入

在类的构造函数中，声明对 `ILoggingService` 工厂的依赖。

```csharp
using AutoLaunchController.Services;
using Serilog;

public class MyNewService
{
    private readonly ILogger _logger;

    public MyNewService(ILoggingService loggingServiceFactory)
    {
        // ...见步骤 2
    }
}
```

### 步骤 2：创建记录器实例

在构造函数内部，调用工厂的 `ForContext<T>()` 方法来创建当前类专属的日志记录器。

```csharp
// ...构造函数内部
public MyNewService(ILoggingService loggingServiceFactory)
{
    _logger = loggingServiceFactory.ForContext<MyNewService>();
}
```
**注**: `ForContext<T>()` 的类型参数 `T` 必须是当前类的类型。

### 步骤 3：记录日志

使用已创建的 `_logger` 字段，在方法的任意位置调用标准日志记录方法。

```csharp
public void ExecuteTask()
{
    _logger.Information("任务开始执行。");
    try
    {
        // ... 业务逻辑 ...
        _logger.Debug("任务细节检查通过。");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "任务执行失败。");
    }
}
```

## 4. 日志级别

请根据以下规范使用日志级别：

*   `Debug`: 用于记录详细的、仅在开发和调试阶段需要关注的诊断信息。
*   `Information`: 用于记录应用程序正常运行过程中的关键事件，如服务启动、任务完成等。
*   `Warning`: 用于记录可预见的、非关键性的问题，程序仍可继续执行。
*   `Error`: 用于记录导致特定功能失败的错误，但应用程序本身未崩溃。
*   `Fatal`: 用于记录导致应用程序提前终止的严重错误。

## 5. 日志输出

日志将被输出到以下两个位置：

1.  **控制台**: 在开发环境中，所有日志将实时输出到 IDE 的输出窗口。
2.  **文件**: 所有日志将根据配置文件中的级别，写入到运行目录下的 `/data/Logs/` 文件夹内。日志文件按天轮转。