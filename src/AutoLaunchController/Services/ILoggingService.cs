using Serilog;
using System; // 添加System命名空间以使用Exception类型

namespace AutoLaunchController.Services
{
    /// <summary>
    /// 日志服务接口，用于跨层提供日志功能而不违反架构约束
    /// </summary>
    public interface ILoggingService
    {
        ILogger Logger { get; }
        void LogInformation(string messageTemplate, params object[] properties);
        void LogWarning(string messageTemplate, params object[] properties);
        void LogError(string messageTemplate, params object[] properties);
        void LogError(Exception exception, string messageTemplate, params object[] properties);
        void LogDebug(string messageTemplate, params object[] properties);
        void LogCritical(string messageTemplate, params object[] properties);
        void LogCritical(Exception exception, string messageTemplate, params object[] properties);
    }
}