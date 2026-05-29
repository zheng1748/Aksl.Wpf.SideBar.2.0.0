using System;
using System.Threading.Tasks;

using Prism.Services.Dialogs;

using Aksl.Dialogs.Views;

namespace Aksl.Dialogs.Services
{
    public interface IDialogViewService
    {
        Task AlertAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "Ok", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);

        Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "Ok", string cancelText = "Cancel", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null);
    }

    public class DialogViewService : IDialogViewService
    {
        #region Members
        private readonly IDialogService _dialogService;
        #endregion

        #region Constructors
        public DialogViewService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }
        #endregion

        public Task AlertAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "Ok", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null)
        {
            var parameters = new DialogParameters { { "IsConfirm", false }, { "Message", message }, { "Title", title }, { "WindowCloseButtonVisibility", windowCloseButtonVisibility }, { "Width", width }, { "Height", height }, { "OkText", okText }};

            _dialogService.ShowDialog(nameof(Views.ConfirmView), parameters, callBack, windowName);

            //  _dialogService.Alert(message: message, title: title, width: width, height: height, okText: okText, callBack: callBack, windowName: nameof(FixedSizeDialogWindow));

            return Task.CompletedTask;
        }

        public Task ConfirmAsync(string message, string title = null, string windowCloseButtonVisibility = "Collapsed", double width = 650d, double height = 300d, string okText = "Ok", string cancelText = "Cancel", string windowName = nameof(FixedSizeDialogWindow), Action<IDialogResult> callBack = null)
        {
            var parameters = new DialogParameters { {"Title",  title },  { "IsConfirm",true }, { "Message", message },{ "WindowCloseButtonVisibility", windowCloseButtonVisibility}, { "Width", width }, { "Height", height },
                                                    { "OkText",  okText }, {"CancelText",cancelText  } };

            _dialogService.ShowDialog(nameof(Views.ConfirmView), parameters, callBack, windowName);

            // _dialogService.Confirm(message: message, title: title, width: width, height: height, okText: okText, cancelText: cancelText, callBack: callBack, windowName: nameof(FixedSizeDialogWindow));

            return Task.CompletedTask;
        }
    }

    public static class DialogExtensions
    {
        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title)
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message,title: title);
            }
        }

        public static async Task AlertWhenAsync(this IDialogViewService dialogViewService, string message, string title, string okText = "Ok")
        {
            if (!string.IsNullOrEmpty(message) || !string.IsNullOrWhiteSpace(message))
            {
                await dialogViewService.AlertAsync(message,title: title, okText: okText);
            }
        }
    }
}
