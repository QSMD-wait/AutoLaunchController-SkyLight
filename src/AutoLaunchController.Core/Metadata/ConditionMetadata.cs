using System;

namespace AutoLaunchController.Core.Metadata;

/// <summary>
/// 判断器模块的元数据。
/// </summary>
public class ConditionMetadata : IModuleMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Version Version { get; set; } = new(1, 0, 0);
}