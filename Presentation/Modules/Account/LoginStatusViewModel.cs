using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.Dialogs.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

using Aksl.Modules.Account.Views;

namespace Aksl.Modules.Account.ViewModels
{
    public class LoginStatusViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        #endregion

        #region Constructors
        public LoginStatusViewModel()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            CreateSignInCommand();
            CreateSignOutCommand();

            RegisterSignInedEvent();

            RegisterPropertyChanged();

            Title = "Please Login";
        }
        #endregion

        #region Properties
        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty<string>(ref _title, value);
        }

        private string? _userName;
        public string? UserName
        {
            get => _userName;
            set => SetProperty<string>(ref _userName, value);
        }

        private bool _isSignIning = false;
        public bool IsSignIning
        {
            get => _isSignIning;
            set
            {
                SetProperty<bool>(ref _isSignIning, value);
                //if (SetProperty<bool>(ref _isSignIning, value))
                //{
                //    (SignInCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                //}
            }
        }

        private bool _isAuthenticated = false;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set => SetProperty<bool>(ref _isAuthenticated, value);
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (sender is LoginStatusViewModel lsvm)
                {
                    if (e.PropertyName == nameof(IsSignIning))
                    {
                        (SignInCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            };
        }
        #endregion

        #region Register SignIned Event
        private void RegisterSignInedEvent()
        {
            _eventAggregator.GetEvent<OnSignInedEvent>().Subscribe((siet) =>
            {
                IsSignIning = false;

                UserName = siet.UserName;
                IsAuthenticated = siet.IsSuccessful;

                //RaisePropertyChanged(nameof(Title));
                Title = null;
                Title = IsAuthenticated ? $"{UserName} Login" : "Please Login";

            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region SignIn Command
        public ICommand SignInCommand { get; private set; }

        private void CreateSignInCommand()
        {
            SignInCommand = new DelegateCommand(async () =>
            {
                await ExecuteSignInCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteSignInCommand();
                return canExecute;
            });
        }

        private async Task ExecuteSignInCommandAsync()
        {
            IsSignIning = true;

            try
            {
                var shellContentActiveContentViewModel = PrismIocExtensions.GetContainer().
                                                           Resolve<ActiveContents.ViewModels.ActiveContentViewModel>(name: ActiveContentNames.ShellContent);
                shellContentActiveContentViewModel.SetSelectedItemByName(nameof(LoginView));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Sign In Failure:");
            }

            //IsSignIning = false;
        }

        private bool CanExecuteSignInCommand()
        {
            return !IsSignIning;
        }
        #endregion

        #region SignOut Command
        //AuthenticationService
        public ICommand SignOutCommand { get; private set; }

        private void CreateSignOutCommand()
        {
            SignOutCommand = new DelegateCommand(async () =>
            {
                await ExecuteSignOutCommandAsync();
            },
            () =>
            {
                var canExecute = CanExecuteSignOutCommand();
                return canExecute;
            });
        }

        private async Task ExecuteSignOutCommandAsync()
        {
            IsSignIning = true;

            try
            {
                var loginOutResponse = await ServiceExtensions.GetLoginHandler().ExecuteLoginOutAction(UserName);

                //if (loginOutResponse.Succeeded)
                //{
                IsAuthenticated = false;
                UserName = null;
                Title = "Please Login";

                RaisePropertyChanged(nameof(UserName));
                //}
                //else
                //{
                //}
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync($"{ex.Message}", "Sign In Failure:");
            }

            IsSignIning = false;
        }

        private bool CanExecuteSignOutCommand()
        {
            return !IsSignIning;
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
        #endregion
    }
}
