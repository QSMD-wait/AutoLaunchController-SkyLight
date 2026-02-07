using AutoLaunchController.Infrastructure.Bootstrap;
using AutoLaunchController.Infrastructure.Configuration.Models;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.File; // 明确引用 RollingInterval 所在的命名空间
using System;
using System.IO;

namespace AutoLaunchController.Infrastructure.Logging
{
    /// <summary>
    /// 负责初始化和管理 Serilog 日志系统。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     [使用场景]：在应用程序启动时，用于根据配置初始化全局日志系统。这是应用程序能够记录运行状态和错误信息的基础。
    /// </para>
    /// <para>
    ///     [工作原理]：您可以把这个类想象成“应用程序的记事本管理员”。它负责设置两个“记事本”：一个实时显示在控制台（方便开发调试），另一个保存在文件中（用于长期归档和分析）。
    /// </para>
    /// <para>
    ///     [最佳实践]：建议在目录初始化之后、其他业务逻辑开始之前调用 <see cref="Initialize"/> 方法。这样确保应用程序从启动到退出的所有活动都能被正确记录。
    /// </para>
    /// </remarks>
    public static class LogManager
    {
        private static readonly string LogDirectory = Path.Combine(DirectoryManager.BaseDirectory, "data", "Logs");

        /// <summary>
        /// 根据应用程序配置初始化全局日志记录器。
        /// </summary>
        /// <param name="settings">应用程序的设置。</param>
        /// <remarks>
        /// <para>
        ///     [使用场景]：在应用程序启动流程中，紧接在目录初始化之后调用，用于配置日志系统的输出目标和详细级别。
        /// </para>
        /// <para>
        ///     [工作原理]：这个方法会根据配置中的日志级别，同时配置两个输出目标：控制台（用于实时监控）和滚动文件（用于持久化存储，每天一个文件，最多保留7天）。
        /// </para>
        /// <para>
        ///     [最佳实践]：建议在生产环境中将日志级别设置为 "Information" 或更高，以避免产生过多调试日志。在开发环境中可以使用 "Debug" 级别以获得更详细的运行信息。
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">当 <paramref name="settings"/> 为 null 时抛出。</exception>
        public static void Initialize(AppSettings settings)
        {
            var logPath = Path.Combine(LogDirectory, "ALC-.log");
            var logLevel = GetLogLevel(settings.LogLevel);

            // 定义日志输出格式，包含了要求的 [日期 时间] 和 [模块] 部分
            const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u3}][{SourceContext}]{Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .Enrich.FromLogContext() // 启用 SourceContext 功能
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File(logPath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: outputTemplate,
                    encoding: System.Text.Encoding.UTF8)
                .CreateLogger();
        }

        /// <summary>
        /// 安全地关闭和刷新日志记录器，确保所有日志都被写入文件。
        /// </summary>
        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }

        /// <summary>
        /// 将字符串形式的日志级别转换为 Serilog 的 LogEventLevel 枚举。
        /// </summary>
        private static LogEventLevel GetLogLevel(string logLevel)
        {
            return logLevel.ToLower() switch
            {
                "debug" => LogEventLevel.Debug,
                "info" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                _ => LogEventLevel.Information // 默认级别
            };
        }
    }
}