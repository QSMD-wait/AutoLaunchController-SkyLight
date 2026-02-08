namespace AutoLaunchController.Core.Metadata;

/// <summary>
/// 为所有可插拔模块（触发器、条件、动作等）提供标准元数据的接口。
/// 元数据用于插件发现、UI生成、安全检查和配置管理。
/// </summary>
public interface IModuleMetadata
{
    /// <summary>
    /// 获取模块的显示名称，用于UI展示。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 获取模块的详细描述，解释其功能和用途。
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 获取模块的版本号。
    /// </summary>
    Version Version { get; }
}