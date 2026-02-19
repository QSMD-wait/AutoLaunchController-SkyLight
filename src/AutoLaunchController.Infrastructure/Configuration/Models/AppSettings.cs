namespace AutoLaunchController.Infrastructure.Configuration.Models
{
    /// <summary>
    /// 定义应用程序的所有可配置设置项。
    /// 这个类的实例将被序列化为 settings.json 文件。
    /// </summary>
    /// <remarks>
    /// <para>
    ///     [使用场景]：作为应用程序配置的数据模型，用于在内存和磁盘（settings.json 文件）之间传递配置信息。
    /// </para>
    /// <para>
    ///     [工作原理]：您可以把这个类想象成“配置信息的容器”。它定义了应用程序可以有哪些设置项，每个设置项的类型和默认值。当应用程序启动时，<see cref="ConfigManager"/> 会从磁盘读取或创建这个类的实例。
    /// </para>
    /// <para>
    ///     [最佳实践]：建议为每个属性提供合理的默认值，这样即使配置文件不存在或损坏，应用程序也能以默认配置正常运行。当需要添加新的配置项时，只需在此类中添加新的属性即可。
    /// </para>
    /// </remarks>
    public class AppSettings
    {
        /// <summary>
        /// 获取或设置应用程序的启动模式。可能的值包括 "Normal"（正常模式，显示主窗口）和 "Silent"（静默模式，在后台运行而不显示窗口）。默认值为 "Normal"。
        /// </summary>
        public string StartupMode { get; set; } = "Normal";

        /// <summary>
        /// 获取或设置日志记录的级别。可能的值包括 "Debug"、"Info"、"Warning"、"Error"、"Fatal"（不区分大小写）。默认值为 "Info"。
        /// </summary>
        public string LogLevel { get; set; } = "Info";

        /// <summary>
        /// 获取或设置是否在开机时自动启动。true 表示应用程序会在操作系统启动时自动运行；false 表示需要手动启动。默认值为 false。
        /// </summary>
        public bool StartOnSystemBoot { get; set; } = false;

        /// <summary>
        /// 获取或设置日志文件的最长保留天数。超过此天数的旧日志文件（包括压缩的 .gz 文件）将在程序启动时被自动删除。默认值为 180 天。
        /// </summary>
        public int LogRetentionDays { get; set; } = 180;

        /// <summary>
        /// 获取或设置日志目录允许占用的最大磁盘空间（单位：MB）。如果目录总大小超过此限制，程序启动时将从最旧的日志文件开始删除，直到满足空间要求。默认值为 512 MB。
        /// </summary>
        public int MaxLogDirectorySizeMB { get; set; } = 512;

        /// <summary>
        /// 获取或设置应用程序的单例运行配置。
        /// </summary>
        public SingleInstanceSettings SingleInstance { get; set; } = new();
    }

    /// <summary>
    /// 定义应用程序的单例（互斥）运行策略。
    /// </summary>
    public enum SingleInstanceStrategy
    {
        /// <summary>
        /// 允许多个实例同时运行。
        /// </summary>
        AllowMultiple,

        /// <summary>
        /// 只允许一个实例。当新实例启动时，会弹出一个消息框提示用户，然后新实例将退出。
        /// </summary>
        ShowMessageAndExit,

        /// <summary>
        /// 只允许一个实例。当新实例启动时，它会不做任何提示并立即退出。
        /// </summary>
        SilentExit
    }

    /// <summary>
    /// 包含与应用程序单例运行相关的所有配置。
    /// </summary>
    public class SingleInstanceSettings
    {
        /// <summary>
        /// 获取或设置单例运行的策略。
        /// 默认值为 <see cref="SingleInstanceStrategy.ShowMessageAndExit"/>。
        /// </summary>
        public SingleInstanceStrategy Strategy { get; set; } = SingleInstanceStrategy.ShowMessageAndExit;

        // ReSharper disable once UnusedMember.Global
        // 预留给未来实现更复杂的消息通知机制
        public MessageConfiguration MessageConfig { get; set; } = new();
    }

    /// <summary>
    /// 为 ShowMessageAndExit 策略预留的详细消息配置。
    /// </summary>
    /// <remarks>
    /// 当前版本暂未使用，为未来扩展保留。
    /// </remarks>
    public class MessageConfiguration
    {
        /// <summary>
        /// 消息窗口的样式，例如 "MessageBox" 或 "Toast"。
        /// </summary>
        public string Style { get; set; } = "MessageBox";

        /// <summary>
        /// 窗口是否自动关闭。
        /// </summary>
        public bool AutoClose { get; set; } = false;

        /// <summary>
        /// 自动关闭的延迟时间（毫秒）。
        /// </summary>
        public int DurationMilliseconds { get; set; } = 3000;

        /// <summary>
        /// 是否确保只显示一个提示窗口（防止快速点击时弹出多个）。
        /// </summary>
        public bool EnsureSingleWindow { get; set; } = true;
    }
}