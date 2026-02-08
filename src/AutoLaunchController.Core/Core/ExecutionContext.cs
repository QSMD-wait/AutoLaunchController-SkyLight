using System.Collections.Generic;
using System.Threading;

namespace AutoLaunchController.Core;

/// <summary>
/// 封装了自动化流程在单次执行过程中的所有状态信息。
/// 这是整个自动化系统的“血液”，在触发、判断、执行等所有节点间传递。
/// </summary>
public class ExecutionContext
{
    /// <summary>
    /// 获取触发当前执行流程的原始事件。
    /// 此属性在流程开始时被初始化，并且是只读的，以确保事件的不可变性。
    /// </summary>
    public TriggerEvent? Event { get; init; }

    /// <summary>
    /// 获取一个可读写的字典，用于在流程节点之间存储和传递自定义变量。
    /// 例如，一个Action的输出可以存储在这里，供后续的Action或Condition使用。
    /// </summary>
    public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

    /// <summary>
    /// 获取一个字典，用于存储系统级的环境变量或状态。
    /// 这些信息通常由系统提供，例如当前CPU使用率、网络状态等，流程中通常只读不写。
    /// </summary>
    public IDictionary<string, object> Environment { get; } = new Dictionary<string, object>();

    /// <summary>
    /// 获取一个取消令牌，用于在需要时优雅地中止当前执行流程。
    /// Flow Engine可以通过此令牌来请求中断、暂停或取消操作。
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}