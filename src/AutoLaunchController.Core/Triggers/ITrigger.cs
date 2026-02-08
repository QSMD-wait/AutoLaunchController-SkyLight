using System;
using AutoLaunchController.Core.Metadata;

namespace AutoLaunchController.Core.Triggers;

/// <summary>
/// 定义了触发器的核心行为，触发器是自动化流程的事件源和入口。
/// </summary>
public interface ITrigger
{
    /// <summary>
    /// 获取触发器的唯一标识符。
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 获取触发器的可读名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取与此触发器关联的元数据。
    /// </summary>
    TriggerMetadata Metadata { get; }

    /// <summary>
    /// 启动触发器，使其开始监听事件。
    /// </summary>
    void Start();

    /// <summary>
    /// 停止触发器，使其停止监听事件。
    /// </summary>
    void Stop();

    /// <summary>
    /// 当触发器检测到符合条件的事件时发生。
    /// </summary>
    event EventHandler<TriggerEvent> Triggered;
}