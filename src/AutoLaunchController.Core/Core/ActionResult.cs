using System.Collections.Generic;

namespace AutoLaunchController.Core;

/// <summary>
/// 表示执行器（IAction）执行后返回的结果。
/// 使用 record 类型确保其不可变性。
/// </summary>
/// <param name="Success">指示执行是否成功。</param>
/// <param name="Message">提供关于执行结果的描述性信息，例如成功消息或错误详情。</param>
/// <param name="Data">一个只读字典，包含执行器产生的输出数据，可供流程后续步骤使用。</param>
public record ActionResult(
    bool Success,
    string? Message = null,
    IReadOnlyDictionary<string, object>? Data = null
);