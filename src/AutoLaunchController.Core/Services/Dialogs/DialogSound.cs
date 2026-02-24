namespace AutoLaunchController.Core.Services.Dialogs
{
    /// <summary>
    /// 定义弹窗播放的声音。
    /// </summary>
    public enum DialogSound
    {
        /// <summary>
        /// 不播放任何声音。
        /// </summary>
        None,

        /// <summary>
        /// 根据弹窗图标自动选择合适的系统声音。
        /// </summary>
        Auto,

        /// <summary>
        /// 播放系统提示音。
        /// </summary>
        Beep,

        /// <summary>
        /// 播放系统感叹声。
        /// </summary>
        Exclamation,

        /// <summary>
        /// 播放系统手型图标声音。
        /// </summary>
        Hand,

        /// <summary>
        /// 播放系统问题声音。
        /// </summary>
        Question
    }
}