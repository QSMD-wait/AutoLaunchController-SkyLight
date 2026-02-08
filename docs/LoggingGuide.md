# AutoLaunchController 日志系统指南 (v2.0)

## 1. 概述

本文档为 `AutoLaunchController` 项目当前日志系统的使用与配置指南。系统基于 **Serilog** 构建，通过一个全局静态类 `LogManager` 进行统一管理，实现了日志的自动维护、灵活配置和便捷使用。

## 2. 核心特性

### 2.1. 日志文件

-   **命名规则**：每次应用程序启动都会生成一个独立的日志文件，命名格式为 `ALC_log-yyyy-MM-dd-HH-mm-ss-revision.log`。
    -   `yyyy-MM-dd-HH-mm-ss` 是日志创建的精确时间。
    -   `-revision` (如 `-1`, `-2`) 是一个可选的修订号，仅在同一秒内启动多个实例时出现，以防止文件名冲突。
-   **存储位置**：所有日志文件（包括压缩包）都存储在程序运行目录下的 `data/Logs/` 文件夹内。

### 2.2. 自动维护

在每次应用程序启动时，`LogManager` 会自动执行以下维护任务：

1.  **压缩**：将上一次运行遗留的所有 `.log` 文件压缩为 `.gz` 格式，以节约磁盘空间。空日志文件会被直接删除。
2.  **按日期清理**：删除所有创建时间早于设定天数（`LogRetentionDays`）的日志文件（包括 `.gz` 文件）。
3.  **按空间清理**：在日期清理后，检查日志目录的总大小。如果超过设定阈值（`MaxLogDirectorySizeMB`），将从最旧的文件开始逐一删除，直到目录大小符合限制。

### 2.3. 配置

日志系统的所有行为都可以通过 `data/settings.json` 文件进行配置。如果文件或特定配置项不存在，系统将使用默认值。

```json
{
  "LogLevel": "Information",
  "LogRetentionDays": 180,
  "MaxLogDirectorySizeMB": 512
}
```

-   `LogLevel`: 日志记录的最低级别。可选值：`Debug`, `Information`, `Warning`, `Error`, `Fatal`。**默认值**: `Information`。
-   `LogRetentionDays`: 日志文件的最长保留天数。**默认值**: `180`。
-   `MaxLogDirectorySizeMB`: 日志目录的最大允许占用空间（单位MB）。**默认值**: `512`。

## 3. 使用方法

得益于全局静态的 `LogManager`，在项目的任何地方记录日志都非常简单。

### 步骤1：获取记录器实例

在需要记录日志的类中，通过 `Log.ForContext<T>()` 方法获取一个绑定了当前类上下文的记录器实例。推荐将其存储在一个私有只读字段中。

```csharp
using Serilog;

public class MyAwesomeService
{
    private readonly ILogger _logger = Log.ForContext<MyAwesomeService>();

    // ...
}
```

### 步骤2：记录日志

使用创建的 `_logger` 实例，调用标准方法记录日志。

```csharp
public void DoWork()
{
    _logger.Information("开始执行重要任务。");
    try
    {
        // ... 业务逻辑 ...
        _logger.Debug("任务细节检查通过。");
        _logger.Information("重要任务执行完毕。");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "执行重要任务时发生意外错误。");
    }
}
```

## 4. 日志级别规范

-   `Debug`: 仅在开发和调试阶段需要关注的详细诊断信息。
-   `Information`: 应用程序正常运行过程中的关键事件（如服务启动、任务完成）。
-   `Warning`: 可预见的、非关键性的问题，程序仍可继续执行。
-   `Error`: 导致特定功能失败的错误，但应用程序本身未崩溃。
-   `Fatal`: 导致应用程序提前终止的严重错误。

## 5. 核心模块索引

下表列出了与日志系统直接相关的核心文件及其职责：

| 文件路径                                                                                   | 职责说明                                                                                                                               |
| ------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------- |
| `src/AutoLaunchController.Infrastructure/Logging/LogManager.cs`                            | **日志系统核心**。负责初始化、配置、自动维护（压缩、清理）以及提供全局日志记录器。是整个日志功能的中枢。                       |
| `src/AutoLaunchController.Infrastructure/Configuration/ConfigManager.cs`                   | **配置管理器**。负责从 `settings.json` 文件中加载日志级别、保留天数等配置。当配置不存在时，它会创建并加载默认配置。         |
| `src/AutoLaunchController.Infrastructure/Configuration/Models/AppSettings.cs` | **配置模型**。定义了 `settings.json` 文件的C#对象模型，包含了 `LogLevel`, `LogRetentionDays` 等日志相关属性及其默认值。 |