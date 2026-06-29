using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Unity;

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
            _dialogViewService = PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();

            _errors = new();

            RegisterPropertyChanged();
        }
        #endregion

        #region Properties
        private string _userName="zhengming";
        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("^[a-zA-Z]{1}([a-zA-Z0-9]){3,15}$", ErrorMessage = "用户名必须是4到16个字母或者数字,且以字母开头.")]
        public string UserName
        {
            get => _userName;
            set => SetProperty<string>(ref _userName, value);
        }

        private string _userNameWater = "UserName";
        public string UserNameWater
        {
            get => _userNameWater;
            set => SetProperty(ref _userNameWater, value);
        }

        private string _password;
        [Required(ErrorMessage = "密码不能为空")]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[$@$!%#?&])[a-zA-Z\d$@$!%#?&]{8,}$", ErrorMessage = "密码至少8个字符,必须包含一个字母,一个数字,一个特殊字符.")]
        public string Password
        {
            get => _password;
            set => SetProperty<string>(ref _password, value);
        }

        private string _passwordWater = "Password";
        public string PasswordWater
        {
            get => _passwordWater;
            set => SetProperty(ref _passwordWater, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
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

        #region IDialogAware
        public override event Action<IDialogResult> RequestClose;

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);

            Title = parameters.GetValue<string>("Title") ?? "请 登 陆";
            WindowCloseButtonVisibility = GetWindowCloseButtonVisibility(parameters.GetValue<string>("WindowCloseButtonVisibility"),Visibility.Visible);
            Width = GetDoubleValue(parameters.GetValue<string>("Width"), 650d);
            Height = GetDoubleValue(parameters.GetValue<string>("Height"), 350d);
            OkText = parameters.GetValue<string>("OkText") ?? "登陆";
            OkIconKind = GetPackIconKind(parameters.GetValue<string>("OkIconKind"), PackIconKind.AccountAdd);
            OkToolTip = parameters.GetValue<string>("OkToolTip") ?? "登 陆";
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
                StatusMessage = "Logining....";

                //var loginHandler = ServiceExtensions.GetLoginHandler();
                //var loginResponse= await loginHandler.LoginAsync(userName: UserName, password: Password);

                //var resetLockoutResponse = await ServiceExtensions.GetLoginHandler().ExecuteResetLockoutAsync(UserName);

                var loginResponse = await ServiceExtensions.GetLoginHandler().ExecuteLoginAction(UserName, Password);
                if (loginResponse.Succeeded)
                {
                    var webApiProvider = ServiceExtensions.GetWebApiProvider();
                    Debug.Assert(webApiProvider.HeaderProperties.GetString("Authorization").Contains("Bearer"));

                    //var loginResponse2 = await ServiceExtensions.GetLoginHandler().ExecuteLoginAction(UserName, Password);

                    //var refreshTokenResponse = await ServiceExtensions.GetLoginHandler().
                    //                            ExecuteRefreshTokenAction(webApiProvider.AccessToken, webApiProvider.RefreshToken);

                    //var refreshTokenResponse2 = await ServiceExtensions.GetLoginHandler().
                    //                              ExecuteRefreshTokenAction(webApiProvider.AccessToken, webApiProvider.RefreshToken);

                    //var generateEmailTokenResponse = await ServiceExtensions.GetLoginHandler().
                    //                            ExecuteGetEmailConfirmationTokenAction([new HttpQueryKeyValuePair("email", "13529805@qq.com")]);


                    IsSuccessful = true;

                    ButtonResult buttonResult = ButtonResult.OK;
                    DialogParameters parameters = new()
                    {
                        { "LoginPopupViewModel", this }
                    };

                    RequestClose?.Invoke(new DialogResult(buttonResult, parameters));

                    await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
                }
                else
                {
                    await _dialogViewService.AlertAsync($"{loginResponse.ToString()}", "Login In Failure:");
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Login In Failure:");
            }

            StatusMessage = "";

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
                    await _dialogViewService.AlertAsync(message: $"{ex.Message}", title: "Close Failure:");
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
                await _dialogViewService.AlertAsync($"{ex.Message}", "Login In Failure:");
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
