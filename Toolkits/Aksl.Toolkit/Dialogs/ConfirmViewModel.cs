using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Aksl.Dialogs.ViewModels
{
    public class ConfirmViewModel : DialogAware
    {
        #region Constructors
        public ConfirmViewModel() : base()
        {
        }
        #endregion

        #region Properties
        private bool _isConfirm;
        public bool IsConfirm
        {
            get => _isConfirm;
            set => SetProperty<bool>(ref _isConfirm, value);
        }

        private string _message;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        #endregion

        #region IDialogAware
        public override event Action<IDialogResult> RequestClose;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = parameters.GetValue<string>("Title") ?? "Notification";
            OkText = parameters.GetValue<string>("OkText") ?? "OK";
            CancelText = parameters.GetValue<string>("CancelText") ?? "Cancel";
            Width = parameters.GetValue<double?>("Width") ?? 200d;
            Height = parameters.GetValue<double?>("Height") ?? 100d;

            IsConfirm = parameters.GetValue<bool?>("IsConfirm") ?? true;
            Message = parameters.GetValue<string>("Message");
        }
        #endregion

        #region Ok Command
        protected override async Task ExecuteOkCommandAsync()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
        #endregion

        #region Cancel Command
        protected override async Task ExecuteCancelCommandAsync()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
        }
        #endregion
    }
}
