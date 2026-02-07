using Serilog;

namespace AutoLaunchController.Services
{
    /// <summary>
    /// 提供一个工厂，用于为不同的服务或视图模型创建带有正确上下文的日志记录器。
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// 为指定的源类型创建一个日志记录器。
        /// </summary>
        /// <typeparam name="TSource">日志来源的类型。</typeparam>
        /// <returns>一个配置了正确来源上下文的 ILogger 实例。</returns>
        ILogger ForContext<TSource>();
    }
}