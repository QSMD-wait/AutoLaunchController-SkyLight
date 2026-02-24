namespace AutoLaunchController.Core.Services.Dialogs
{
    /// <summary>
    /// 封装了显示一个消息框所需的所有配置参数。
    /// </summary>
    public class MessageBoxParameters
    {
        /// <summary>
        /// 弹窗标题 (必填)。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 弹窗显示的消息 (必填)。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 要显示的按钮组合。默认为 OK。
        /// </summary>
        public DialogButtonSet Buttons { get; set; } = DialogButtonSet.OK;

        /// <summary>
        /// 要显示的图标。默认为 Information。
        /// </summary>
        public DialogIcon Icon { get; set; } = DialogIcon.Information;

        /// <summary>
        /// 默认获得焦点的按钮。
        /// </summary>
        public DialogDefaultButton DefaultButton { get; set; } = DialogDefaultButton.None;

        /// <summary>
        /// 弹窗时播放的声音。默认为 None (无声)。
        /// </summary>
        public DialogSound Sound { get; set; } = DialogSound.None;

        /// <summary>
        /// 构造一个消息框参数对象。
        /// </summary>
        public MessageBoxParameters(string title, string message)
        {
            Title = title;
            Message = message;
        }
    }
}