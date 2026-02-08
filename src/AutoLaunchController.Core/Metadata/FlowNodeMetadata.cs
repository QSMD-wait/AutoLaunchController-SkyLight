using System;

namespace AutoLaunchController.Core.Metadata;

/// <summary>
/// 流程节点模块的元数据。
/// </summary>
public class FlowNodeMetadata : IModuleMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Version Version { get; set; } = new(1, 0, 0);
    public string NodeType { get; set; } = string.Empty;
}