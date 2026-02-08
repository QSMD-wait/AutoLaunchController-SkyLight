namespace AutoLaunchController.Core;

/// <summary>
/// 定义了模块（如触发器、执行器）操作的敏感级别或风险等级。
/// 用于UI提示、安全控制和配置导入策略。
/// </summary>
public enum SensitivityLevel
{
    /// <summary>
    /// 普通级别，操作不涉及敏感信息或系统关键改动。
    /// </summary>
    Normal,

    /// <summary>
    /// 警告级别，操作可能涉及用户隐私或轻微的系统改动，需要用户注意。
    /// </summary>
    Warning,

    /// <summary>
    /// 高风险级别，操作涉及核心系统设置、文件系统关键区域或敏感数据，需要管理员权限或明确授权。
    /// </summary>
    HighRisk
}