using Serilog;
using System; // 添加System命名空间以使用Exception类型

namespace AutoLaunchController.Services
{
    /// <summary>
    /// 日志服务实现
    /// </summary>
    public class LoggingService : ILoggingService
    {
        public ILogger Logger { get; }

        public LoggingService()
        {
            Logger = Log.ForContext<LoggingService>();
        }

        public void LogInformation(string messageTemplate, params object[] properties)
        {
            Logger.Information(messageTemplate, properties);
        }

        public void LogWarning(string messageTemplate, params object[] properties)
        {
            Logger.Warning(messageTemplate, properties);
        }

        public void LogError(string messageTemplate, params object[] properties)
        {
            Logger.Error(messageTemplate, properties);
        }

        public void LogError(Exception exception, string messageTemplate, params object[] properties)
        {
            Logger.Error(exception, messageTemplate, properties);
        }

        public void LogDebug(string messageTemplate, params object[] properties)
        {
            Logger.Debug(messageTemplate, properties);
        }

        public void LogCritical(string messageTemplate, params object[] properties)
        {
            Logger.Fatal(messageTemplate, properties);
        }

        public void LogCritical(Exception exception, string messageTemplate, params object[] properties)
        {
            Logger.Fatal(exception, messageTemplate, properties);
        }
    }
}