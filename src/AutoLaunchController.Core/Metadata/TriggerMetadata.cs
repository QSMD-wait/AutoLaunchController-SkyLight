namespace AutoLaunchController.Core.Metadata;

/// <summary>
/// 触发器模块的元数据。
/// </summary>
public class TriggerMetadata : IModuleMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Version Version { get; set; } = new(1, 0, 0);
    public SensitivityLevel Sensitivity { get; set; } = SensitivityLevel.Normal;
}