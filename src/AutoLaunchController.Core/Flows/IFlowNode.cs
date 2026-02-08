using System.Threading.Tasks;
using AutoLaunchController.Core.Metadata;

namespace AutoLaunchController.Core.Flows;

/// <summary>
/// 定义了流程节点的核心行为，用于流程编排。
/// </summary>
public interface IFlowNode
{
    /// <summary>
    /// 获取流程节点的唯一标识符。
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 获取流程节点的可读名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取与此流程节点关联的元数据。
    /// </summary>
    FlowNodeMetadata Metadata { get; }

    /// <summary>
    /// 异步执行流程节点定义的逻辑。
    /// </summary>
    /// <param name="context">当前的执行上下文。</param>
    /// <returns>一个表示异步操作的任务。</returns>
    Task ExecuteAsync(ExecutionContext context);
}