using AutoLaunchController.Core.Metadata;

namespace AutoLaunchController.Core.Conditions;

/// <summary>
/// 定义了判断器的核心行为，判断器是流程中的布尔逻辑节点。
/// </summary>
public interface ICondition
{
    /// <summary>
    /// 获取判断器的唯一标识符。
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 获取判断器的可读名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取与此判断器关联的元数据。
    /// </summary>
    ConditionMetadata Metadata { get; }

    /// <summary>
    /// 根据给定的执行上下文评估条件。
    /// </summary>
    /// <param name="context">当前的执行上下文。</param>
    /// <returns>如果条件为真，则返回 true；否则返回 false。</returns>
    bool Evaluate(ExecutionContext context);
}