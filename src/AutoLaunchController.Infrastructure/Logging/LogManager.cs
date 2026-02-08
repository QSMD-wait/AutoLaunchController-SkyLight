using AutoLaunchController.Infrastructure.Bootstrap;
using AutoLaunchController.Infrastructure.Configuration.Models;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace AutoLaunchController.Infrastructure.Logging
{
    /// <summary>
    /// 负责初始化和管理应用程序的日志系统。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     [核心职责]：此类是应用程序日志功能的唯一入口点。它负责在程序启动时配置全局日志记录器（Logger），并执行必要的日志文件维护任务。
    /// </para>
    /// <para>
    ///     [工作流程]：
    ///     1.  **日志维护**：在初始化时，首先会执行一系列维护操作，包括压缩上一次的日志文件（.log -> .gz），并根据配置的保留策略（天数和磁盘空间）清理旧的日志。
    ///     2.  **生成唯一路径**：为本次应用程序的运行生成一个唯一的、带时间戳和修订号的日志文件路径，以避免文件冲突并清晰地记录每次运行。
    ///     3.  **配置Logger**：最后，配置Serilog的全局静态实例 `Log.Logger`，将所有日志输出到控制台和新生成的唯一日志文件中。
    /// </para>
    /// <para>
    ///     [最佳实践]：应在应用程序启动的最早期（在 `App.xaml.cs` 的 `OnStartup` 中，紧随 `DirectoryManager` 和 `ConfigManager` 初始化之后）调用 <see cref="Initialize"/> 方法。
    /// </para>
    /// </remarks>
    public static class LogManager
    {
        private static readonly string LogDirectory = Path.Combine(DirectoryManager.BaseDirectory, "data", "Logs");

        /// <summary>
        /// 初始化全局日志记录器。此方法会执行日志维护（压缩、清理），然后为本次运行创建一个新的日志文件。
        /// </summary>
        /// <param name="settings">应用程序的设置。即使此参数为null或其中的日志相关配置缺失，系统也会采用默认值安全地完成初始化。</param>
        /// <remarks>
        /// <para>
        ///     [健壮性]：此方法被设计为高度健壮。即使配置文件加载失败（传入的 <paramref name="settings"/> 为 null），或者在日志维护期间发生单个文件操作错误，它也会尽力完成初始化，确保日志系统可用。
        /// </para>
        /// </remarks>
        public static void Initialize(AppSettings settings)
        {
            // 即使配置加载失败，也使用默认配置保证日志功能可用
            var effectiveSettings = settings ?? new AppSettings();

            try
            {
                PerformLogMaintenance(effectiveSettings);
            }
            catch (Exception ex)
            {
                // 在日志系统完全初始化前，使用临时的控制台日志记录维护期间的严重错误
                Console.WriteLine($"[FATAL] Log maintenance failed: {ex}");
            }

            var logPath = GenerateUniqueLogPath();
            var logLevel = GetLogLevel(effectiveSettings.LogLevel);

            const string outputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss}][{Level:u3}][{SourceContext}]{Message:lj}{NewLine}{Exception}";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(logLevel)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: outputTemplate)
                .WriteTo.File(logPath,
                    outputTemplate: outputTemplate,
                    encoding: Encoding.UTF8) // 不再需要滚动相关的参数
                .CreateLogger();
            
            Log.ForContext(typeof(LogManager)).Information("日志系统初始化成功。日志路径：{LogPath}", logPath);
        }

        /// <summary>
        /// 执行日志目录的维护操作，包括压缩旧日志和根据策略清理日志。
        /// </summary>
        /// <param name="settings">包含日志保留策略（天数和大小）的应用程序设置。</param>
        private static void PerformLogMaintenance(AppSettings settings)
        {
            if (!Directory.Exists(LogDirectory)) return;

            // 1. 压缩上次的日志文件
            var logFilesToCompress = Directory.GetFiles(LogDirectory, "*.log");
            foreach (var logFile in logFilesToCompress)
            {
                try
                {
                    CompressLogFile(logFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to compress log file {logFile}: {ex.Message}");
                }
            }

            // 2. 根据保留策略清理日志
            var allLogs = Directory.GetFiles(LogDirectory, "*.*", SearchOption.AllDirectories)
                                   .Select(f => new FileInfo(f))
                                   .OrderBy(f => f.CreationTime)
                                   .ToList();

            // 2a. 按时间清理
            var retentionDays = settings.LogRetentionDays;
            var timeCutoff = DateTime.Now.AddDays(-retentionDays);
            var logsToDeleteByTime = allLogs.Where(f => f.CreationTime < timeCutoff).ToList();

            foreach (var log in logsToDeleteByTime)
            {
                try
                {
                    log.Delete();
                    allLogs.Remove(log); // 从列表中移除，避免重复计算
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[错误] 删除旧日志文件 {log.Name} 失败: {ex.Message}");
                }
            }

            // 2b. 按空间清理
            long maxDirectorySize = (long)settings.MaxLogDirectorySizeMB * 1024 * 1024;
            long currentSize = allLogs.Sum(f => f.Length);

            while (currentSize > maxDirectorySize && allLogs.Any())
            {
                var oldestLog = allLogs.First();
                try
                {
                    var fileSize = oldestLog.Length;
                    oldestLog.Delete();
                    currentSize -= fileSize;
                    allLogs.RemoveAt(0);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Failed to delete log file {oldestLog.Name} for size limit: {ex.Message}");
                    break; // 如果删除失败，退出循环以避免死循环
                }
            }
        }

        /// <summary>
        /// 将指定的日志文件压缩为 .gz 格式，并删除原文件。
        /// </summary>
        /// <param name="filePath">要压缩的日志文件的完整路径。</param>
        private static void CompressLogFile(string filePath)
        {
            var originalFile = new FileInfo(filePath);
            if (!originalFile.Exists || originalFile.Length == 0)
            {
                // 如果文件不存在或为空，直接删除
                try { originalFile.Delete(); } catch { /* 忽略删除失败 */ }
                return;
            }

            using (var originalFileStream = originalFile.OpenRead())
            {
                using (var compressedFileStream = File.Create(filePath + ".gz"))
                {
                    using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                    {
                        originalFileStream.CopyTo(compressionStream);
                    }
                }
            }
            originalFile.Delete();
        }

        /// <summary>
        /// 生成一个本次运行唯一的日志文件路径，能处理并发启动。
        /// </summary>
        /// <returns>唯一的日志文件绝对路径。</returns>
        private static string GenerateUniqueLogPath()
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var baseName = $"ALC_log-{timestamp}";
            var logPath = Path.Combine(LogDirectory, $"{baseName}.log");

            int revision = 0;
            // 附加修订号以处理同一秒内多次启动的罕见情况
            while (File.Exists(logPath))
            {
                revision++;
                logPath = Path.Combine(LogDirectory, $"{baseName}-{revision}.log");
            }
            return logPath;
        }

        /// <summary>
        /// 安全地关闭和刷新日志记录器，确保所有挂起的日志都被写入文件。
        /// </summary>
        /// <remarks>
        /// 建议在应用程序正常退出时（例如，在 `App.xaml.cs` 的 `OnExit` 方法中）调用此方法。
        /// </remarks>
        public static void Shutdown()
        {
            Log.CloseAndFlush();
        }

        /// <summary>
        /// 将字符串形式的日志级别转换为 Serilog 的 LogEventLevel 枚举。
        /// </summary>
        /// <param name="logLevel">表示日志级别的字符串（不区分大小写）。</param>
        /// <returns>对应的 <see cref="LogEventLevel"/> 枚举值。如果输入无效，则默认为 <see cref="LogEventLevel.Information"/>。</returns>
        private static LogEventLevel GetLogLevel(string logLevel)
        {
            return logLevel?.ToLower() switch
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