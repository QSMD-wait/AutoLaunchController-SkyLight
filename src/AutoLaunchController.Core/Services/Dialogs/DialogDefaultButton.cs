namespace AutoLaunchController.Core.Services.Dialogs
{
    /// <summary>
    /// 定义弹窗的默认焦点按钮。
    /// </summary>
    public enum DialogDefaultButton
    {
        /// <summary>
        /// 无特定默认值。
        /// </summary>
        None,

        /// <summary>
        /// 将焦点设置在主要操作按钮上 (例如 "OK" 或 "Yes")。
        /// </summary>
        Primary,

        /// <summary>
        /// 将焦点设置在次要或取消按钮上 (例如 "Cancel" 或 "No")。
        /// </summary>
        Secondary
    }
}