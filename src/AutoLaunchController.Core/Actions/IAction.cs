using AutoLaunchController.Core.Metadata;

namespace AutoLaunchController.Core.Actions;

/// <summary>
/// 定义了执行器的核心行为，执行器是系统中执行具体操作的“输出”单元。
/// </summary>
public interface IAction
{
    /// <summary>
    /// 获取执行器的唯一标识符。
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 获取执行器的可读名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取与此执行器关联的元数据。
    /// </summary>
    ActionMetadata Metadata { get; }

    /// <summary>
    /// 执行操作。
    /// </summary>
    /// <param name="context">当前的执行上下文。</param>
    /// <returns>一个包含执行结果的 ActionResult 对象。</returns>
    ActionResult Execute(ExecutionContext context);
}