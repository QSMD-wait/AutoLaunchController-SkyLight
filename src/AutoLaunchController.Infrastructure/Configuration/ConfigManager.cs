using AutoLaunchController.Infrastructure.Bootstrap;
using AutoLaunchController.Infrastructure.Configuration.Models;
using Serilog; // 引入 Serilog 静态 Log 类的命名空间
using System.Text.Json;
using System;
using System.IO;

namespace AutoLaunchController.Infrastructure.Configuration
{
    /// <summary>
    /// 负责加载、保存和初始化应用程序的核心配置 (settings.json)。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     [使用场景]：在应用程序启动时，用于加载或创建应用程序的配置文件。这是应用程序能够根据用户配置运行的基础。
    /// </para>
    /// <para>
    ///     [工作原理]：您可以把这个类想象成“应用程序的配置管家”。它负责管理一个名为 settings.json 的“配置笔记本”，在程序启动时检查这个笔记本是否存在、是否完好，如果不存在或损坏，就会创建一个新的默认笔记本。
    /// </para>
    /// <para>
    ///     [最佳实践]：建议在目录初始化之后、日志系统初始化之前调用 <see cref="LoadOrInitialize"/> 方法。这样确保日志系统能够使用正确的配置进行初始化。
    /// </para>
    /// </remarks>
    public static class ConfigManager
    {
        private static readonly string DataDirectory = Path.Combine(DirectoryManager.BaseDirectory, "data");
        private static readonly string ConfigFilePath = Path.Combine(DataDirectory, "settings.json");

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true // 让生成的 JSON 文件格式化，便于阅读和调试
        };

        /// <summary>
        /// 加载现有的 settings.json 配置文件，如果文件不存在或损坏，则创建并加载一份默认配置。
        /// </summary>
        /// <returns>加载或新创建的 AppSettings 实例。</returns>
        /// <remarks>
        /// <para>
        ///     [使用场景]：在应用程序启动流程中，紧接在目录初始化之后调用，用于获取应用程序的配置信息。
        /// </para>
        /// <para>
        ///     [工作原理]：这个方法会检查 settings.json 文件是否存在且有效。如果文件不存在、内容为空或格式损坏，它会自动创建一份包含默认值的配置文件。
        /// </para>
        /// <para>
        ///     [最佳实践]：这个方法具有自我修复能力，即使配置文件被意外删除或损坏，应用程序也能正常启动并使用默认配置。这为应用程序的健壮性提供了保障。
        /// </para>
        /// </remarks>
        public static AppSettings LoadOrInitialize()
        {
            if (File.Exists(ConfigFilePath))
            {
                try
                {
                    var json = File.ReadAllText(ConfigFilePath);
                    // 如果文件内容为空，也视为需要初始化的情况
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        return CreateDefaultAndSave();
                    }
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    return settings ?? CreateDefaultAndSave();
                }
                catch (Exception ex) // 捕获读取或反序列化时可能发生的任何异常
                {
                    Log.ForContext(typeof(ConfigManager)).Warning(ex, "加载 settings.json 失败或文件已损坏。已重新创建默认配置文件。");
                    return CreateDefaultAndSave(); // 如果解析失败，就返回一份默认配置
                }
            }
            else
            {
                return CreateDefaultAndSave();
            }
        }

        /// <summary>
        /// 将指定的 AppSettings 对象保存到 settings.json 文件。
        /// </summary>
        /// <param name="settings">要保存的配置对象。</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="settings"/> 为 null 时抛出。</exception>
        public static void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            File.WriteAllText(ConfigFilePath, json);
        }

        /// <summary>
        /// 创建一个默认的 AppSettings 实例并将其保存到磁盘。
        /// </summary>
        /// <returns>新创建的默认 AppSettings 实例。</returns>
        private static AppSettings CreateDefaultAndSave()
        {
            var defaultSettings = new AppSettings();
            Save(defaultSettings);
            return defaultSettings;
        }
    }
}