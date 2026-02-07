using Serilog;

namespace AutoLaunchController.Services
{
    /// <summary>
    /// 日志服务工厂的实现。
    /// </summary>
    public class LoggingService : ILoggingService
    {
        /// <summary>
        /// 为指定的源类型创建一个日志记录器。
        /// </summary>
        /// <typeparam name="TSource">日志来源的类型。</typeparam>
        /// <returns>一个配置了正确来源上下文的 ILogger 实例。</returns>
        public ILogger ForContext<TSource>()
        {
            return Log.ForContext<TSource>();
        }
    }
}