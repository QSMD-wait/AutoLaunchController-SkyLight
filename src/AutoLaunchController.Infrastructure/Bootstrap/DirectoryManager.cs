using System;
using System.Collections.Generic;
using System.IO;

namespace AutoLaunchController.Infrastructure.Bootstrap
{
    /// <summary>
    /// 提供应用程序运行时目录的初始化与管理功能。
    /// 对应设计蓝图中的 "Bootstrap & Self-Healing" 模块。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     [使用场景]：在应用程序启动时，用于创建和管理应用程序运行所需的标准目录结构。这是应用程序能够正常工作的基础前提。
    /// </para>
    /// <para>
    ///     [工作原理]：您可以把这个类想象成“应用程序的档案管理员”。它负责在程序启动时检查并创建所有必要的文件夹，确保每个功能模块（如配置、日志、插件等）都有自己专属的“储物柜”。
    /// </para>
    /// <para>
    ///     [最佳实践]：建议在应用程序启动的最早期（如 <c>App.xaml.cs</c> 的 <c>OnStartup</c> 方法开头）调用 <see cref="InitializeCoreDirectories"/> 方法。这样可以确保后续所有依赖文件系统的操作都能在正确的目录结构下进行。
    /// </para>
    /// </remarks>
    public static class DirectoryManager
    {
        /// <summary>
        /// 获取应用程序的根目录路径。
        /// 通常是主程序 'AutoLaunchController-SkyLight.exe' 所在的目录。
        /// </summary>
        public static string BaseDirectory { get; } = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// 根据设计蓝图定义的标准顶层目录。
        /// </summary>
        private static readonly List<string> TopLevelDirectories = new List<string>
        {
            "app",
            "data"
        };

        /// <summary>
        /// 根据设计蓝图定义的 /data 子目录结构。
        /// </summary>
        private static readonly List<string> DataSubDirectories = new List<string>
        {
            "Config",
            "Plugins", // 'Modules' 已更名为 'Plugins'
            "Temp",
            "Logs",
            "Triggers",
            "Actions"
        };

        /// <summary>
        /// 初始化所有核心目录。
        /// 检查并创建在蓝图中定义的标准目录结构，确保应用程序有正确的运行时环境。
        /// 此操作是幂等的。
        /// </summary>
        /// <remarks>
        /// <para>
        ///     [使用场景]：在应用程序启动时调用，用于确保所有必要的目录都已存在。如果目录已存在，则不会重复创建。
        /// </para>
        /// <para>
        ///     [工作原理]：这个方法会按照设计蓝图中的标准目录结构，依次创建两个层级的目录：首先是顶层目录（app、data），然后在 data 目录下创建子目录（Config、Plugins、Temp、Logs、Triggers、Actions）。
        /// </para>
        /// <para>
        ///     [最佳实践]：由于此方法是幂等的，您可以安全地在应用程序启动时多次调用它，而不会产生副作用。这为应用程序的自我修复能力提供了基础。
        /// </para>
        /// </remarks>
        public static void InitializeCoreDirectories()
        {
            // 1. 创建顶层目录
            foreach (var dirName in TopLevelDirectories)
            {
                var fullPath = Path.Combine(BaseDirectory, dirName);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }

            // 2. 在 /data 目录下创建子目录
            var dataDirectoryPath = Path.Combine(BaseDirectory, "data");
            foreach (var subDirName in DataSubDirectories)
            {
                var fullPath = Path.Combine(dataDirectoryPath, subDirName);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    // 未来可在此处添加目录创建成功的日志记录
                }
            }
        }
    }
}