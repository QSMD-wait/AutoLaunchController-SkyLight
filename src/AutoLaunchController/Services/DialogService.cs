using System.Media;
using System.Threading.Tasks;
using System.Windows;
using AutoLaunchController.Core.Services;
using AutoLaunchController.Core.Services.Dialogs;
using ModernMessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace AutoLaunchController.Services;

/// <summary>
/// <see cref="IDialogService"/> 接口的WPF平台具体实现。
/// </summary>
public class DialogService : IDialogService
{
    public Task<DialogResult> ShowAsync(MessageBoxParameters parameters)
    {
        // 使用 Task.Run 将弹窗操作包裹起来，以便可以 await，同时通过 Dispatcher.Invoke 确保 UI 操作在主线程执行。
        return Task.Run(() => Application.Current.Dispatcher.Invoke(() =>
        {
            var wpfResult = ModernMessageBox.Show(
                parameters.Message,
                parameters.Title,
                TranslateButtons(parameters.Buttons),
                TranslateIcon(parameters.Icon),
                TranslateDefaultButton(parameters.DefaultButton),
                TranslateSound(parameters.Sound, parameters.Icon)
            );

            return TranslateResult(wpfResult);
        }));
    }

    public Task ShowAsync(string title, string message, DialogIcon icon = DialogIcon.Information)
    {
        var parameters = new MessageBoxParameters(title, message)
        {
            Icon = icon
        };
        // 调用更核心的 ShowAsync 方法，并忽略返回值。
        return ShowAsync(parameters);
    }

    #region Private Translation Helpers

    private MessageBoxButton TranslateButtons(DialogButtonSet buttons) => buttons switch
    {
        DialogButtonSet.OK => MessageBoxButton.OK,
        DialogButtonSet.OKCancel => MessageBoxButton.OKCancel,
        DialogButtonSet.YesNo => MessageBoxButton.YesNo,
        DialogButtonSet.YesNoCancel => MessageBoxButton.YesNoCancel,
        _ => MessageBoxButton.OK
    };

    private MessageBoxImage TranslateIcon(DialogIcon icon) => icon switch
    {
        DialogIcon.None => MessageBoxImage.None,
        DialogIcon.Information => MessageBoxImage.Information,
        DialogIcon.Warning => MessageBoxImage.Warning,
        DialogIcon.Error => MessageBoxImage.Error,
        DialogIcon.Question => MessageBoxImage.Question,
        _ => MessageBoxImage.None
    };

    private MessageBoxResult TranslateDefaultButton(DialogDefaultButton defaultButton) => defaultButton switch
    {
        DialogDefaultButton.Primary => MessageBoxResult.OK, // 假设 OK/Yes 是主要按钮
        DialogDefaultButton.Secondary => MessageBoxResult.Cancel, // 假设 Cancel/No 是次要按钮
        _ => MessageBoxResult.None
    };

    private SystemSound? TranslateSound(DialogSound sound, DialogIcon icon)
    {
        return sound switch
        {
            DialogSound.None => null,
            DialogSound.Auto => GetSoundFromIcon(icon),
            DialogSound.Beep => SystemSounds.Beep,
            DialogSound.Exclamation => SystemSounds.Exclamation,
            DialogSound.Hand => SystemSounds.Hand,
            DialogSound.Question => SystemSounds.Question,
            _ => null
        };
    }

    private SystemSound? GetSoundFromIcon(DialogIcon icon)
    {
        return icon switch
        {
            DialogIcon.Warning => SystemSounds.Exclamation,
            DialogIcon.Error => SystemSounds.Hand,
            DialogIcon.Question => SystemSounds.Question,
            _ => SystemSounds.Beep // 默认为普通提示音
        };
    }

    private DialogResult TranslateResult(MessageBoxResult result) => result switch
    {
        MessageBoxResult.None => DialogResult.None,
        MessageBoxResult.OK => DialogResult.OK,
        MessageBoxResult.Cancel => DialogResult.Cancel,
        MessageBoxResult.Yes => DialogResult.Yes,
        MessageBoxResult.No => DialogResult.No,
        _ => DialogResult.None
    };

    #endregion
}