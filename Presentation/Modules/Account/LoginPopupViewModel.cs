using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Prism;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;

namespace Aksl.Modules.Account.ViewModels
{
    public class LoginPopupViewModel : Aksl.Dialogs.DialogAware, IDataErrorInfo
    {
        #region Members
        private readonly IDialogViewService _dialogViewService;
        private readonly Dictionary<string, string> _errors;
        #endregion

        #region Constructors
        public LoginPopupViewModel():base()
        {
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            _errors = new();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        public string WindownTitle { get; private set; } = "Sign In";

        private string _userName;
        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]){3,15}$", ErrorMessage = "用户名必须是4到16个字母或者\n\r数字,且以字母开头.")]
        public string UserName
        {
            get => _userName;
            set => SetProperty<string>(ref _userName, value);
        }

        private string _password;
        [Required(ErrorMessage = "密码不能为空")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[$@$!%#?&])[a-zA-Z\d$@$!%#?&]{8,}$", ErrorMessage = "密码至少8个字符,必须包含一个字母,\n\r一个数字,一个特殊字符.")]
        public string Password
        {
            get => _password;
            set => SetProperty<string>(ref _password, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
            //{
            //    if (SetProperty<bool>(ref _isLoading, value))
            //    {
            //        (LoginCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            //    }
            //}
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool _isSuccessful = false;
        public bool IsSuccessful
        {
            get => _isSuccessful;
            set => SetProperty(ref _isSuccessful, value);
        }

        public bool HasErrors => _errors.Count > 0;
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (sender is LoginPopupViewModel loginPopupViewModel)
                {
                    if (e.PropertyName == nameof(IsLoading) || e.PropertyName == nameof(UserName) || e.PropertyName == nameof(Password))
                    {
                        (OkCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region Properties
        private string _userNameWater = "UserName";
        public string UserNameWater
        {
            get => _userNameWater;
            set => SetProperty(ref _userNameWater, value);
        }

        private string _passwordWater = "Password";
        public string PasswordWater
        {
            get => _passwordWater;
            set => SetProperty(ref _passwordWater, value);
        }
        #endregion

        #region IDialogAware
        public override event Action<IDialogResult> RequestClose;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = parameters.GetValue<string>("Title") ?? "登  陆";
            WindowCloseButtonVisibility = GetWindowCloseButtonVisibility(parameters.GetValue<string>("WindowCloseButtonVisibility"),Visibility.Visible);
            Width = GetDoubleValue(parameters.GetValue<string>("Width"), 650d);
            Height = GetDoubleValue(parameters.GetValue<string>("Height"), 350d);
            OkText = parameters.GetValue<string>("OkText") ?? "登陆";
            OkIconKind = GetPackIconKind(parameters.GetValue<string>("OkIconKind"), PackIconKind.AccountAdd);
            OkToolTip = parameters.GetValue<string>("OkToolTip") ?? "登陆";
            CancelText = parameters.GetValue<string>("CancelText") ?? "Cancel";

            UserNameWater = parameters.GetValue<string>("UserNameWater") ?? "用户名";
            PasswordWater = parameters.GetValue<string>("PasswordWater") ?? "密码";
        }
        #endregion

        #region Ok Command
        protected override async Task ExecuteOkCommandAsync()
        {
            IsLoading = true;

            try
            {
                IsSuccessful = true;

                ButtonResult buttonResult = ButtonResult.None;
                DialogParameters parameters = new()
                {
                    { "LoginPopupViewModel", this }
                };
                buttonResult = ButtonResult.OK;

                RequestClose?.Invoke(new DialogResult(buttonResult, parameters));

                //await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertWhenAsync($"{ex.Message}", "Login In Failure:");
            }

            IsLoading = false;
        }

        protected override bool CanExecuteOkCommand()
        {
            return !IsLoading && !HasErrors;
        }
        #endregion

        #region Close Method
        public async void ExecuteClose(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                IsLoading = true;

                try
                {
                    IsSuccessful = false;

                    ButtonResult buttonResult = ButtonResult.None;
                    DialogParameters parameters = new()
                    {
                        { "LoginPopupViewModel", this }
                    };
                    buttonResult = ButtonResult.Cancel;

                    RequestClose?.Invoke(new DialogResult(buttonResult, parameters));

                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertWhenAsync($"{ex.Message}", "Close Failure:");
                }

                IsLoading = false;
            }
        }
        #endregion

        #region Cancel Command
        protected override async Task ExecuteCancelCommandAsync()
        {
            IsLoading = true;

            try
            {
                IsSuccessful = false;

                ButtonResult buttonResult = ButtonResult.None;
                DialogParameters parameters = new()
                {
                   { "LoginPopupViewModel", this }
                };
                buttonResult = ButtonResult.Cancel;

                RequestClose?.Invoke(new DialogResult(buttonResult, parameters));

                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertWhenAsync($"Please userName and  password", "Login Failure:");
            }

            IsLoading = false;
        }
        #endregion

        #region IDataErrorInfo Interface
        public string this[string columnName]
        {
            get
            {
                ValidationContext validationContext = new(this, null, null)
                {
                    MemberName = columnName
                };
                List<System.ComponentModel.DataAnnotations.ValidationResult> validationResults = new();
                var isValidate = Validator.TryValidateProperty(this.GetType().GetProperty(columnName).GetValue(this, null), validationContext, validationResults);
                if (!isValidate && validationResults.Any())
                {
                    if (!_errors.ContainsKey(columnName))
                    {
                        _errors.Add(columnName, "");
                        // return _errors[columnName];
                    }

                    return string.Join(Environment.NewLine, validationResults.Select(r => r.ErrorMessage).ToArray());
                }
                else
                {
                    _errors.Remove(columnName);
                }

                //RaisePropertyChanged(nameof(HasErrors));

                return null;
            }
            set
            {
                _errors[columnName] = value;
            }
        }

        public string Error => null;
        #endregion
    }
}
