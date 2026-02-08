using System;
using System.Collections.Generic;

namespace AutoLaunchController.Core;

/// <summary>
/// 表示由触发器（ITrigger）生成的结构化事件。
/// 使用 record 类型确保其不可变性，适合在系统中安全地传递。
/// </summary>
/// <param name="Timestamp">事件发生的时间戳。</param>
/// <param name="TriggerId">生成此事件的触发器的唯一ID。</param>
/// <param name="EventType">事件的类型标识符，例如“ProcessStarted”或“WindowFocused”。</param>
/// <param name="Data">一个只读字典，包含与事件相关的附加上下文数据，如进程名、窗口标题等。</param>
public record TriggerEvent(
    DateTime Timestamp,
    string TriggerId,
    string EventType,
    IReadOnlyDictionary<string, object>? Data = null
);