using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.ViewModels;
using Aksl.Modules.Account.Views;
using Aksl.Modules.HamburgerMenuSideBar.ViewModels;
using Aksl.Modules.HamburgerMenuSideBar.Views;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.Shell.ViewModels
{
    public class ShellViewModel : BindableBase
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private readonly IDialogViewService _dialogViewService;
        private object _currentView;
        #endregion

        #region Constructors
        public ShellViewModel()
        {
            _container = PrismIocExtensions.GetContainer().Resolve<IUnityContainer>();
            _serviceProvider = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>();
            _regionManager = PrismIocExtensions.GetContainer().Resolve<IRegionManager>();
            _eventAggregator = PrismIocExtensions.GetContainer().Resolve<IEventAggregator>();
            _dialogViewService = PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();

            RegisterSignInedEvent();
            RegisterAccessTokenExpiredEvent();

            RegisterActiveContents().Await(configureAwait:true);
        }
        #endregion

        #region Properties
        //private RandomActiveContentViewModel _shellContentActiveContentViewModel;
        public RandomActiveContentViewModel ShellContentActiveContentViewModel
        {
            get => field ;
            set => SetProperty<RandomActiveContentViewModel>(ref field, value);
        }

       // public RandomActiveContentViewModel ShellContentActiveContentViewModel { get; set; }

        //   public ActiveContentViewModel LoginActiveContentViewModel { get; set; }
       // public RandomActiveContentViewModel LoginActiveContentViewModel { get; set; }
       // private RandomActiveContentViewModel _loginActiveContentViewModel;
        public RandomActiveContentViewModel LoginActiveContentViewModel
        {
            get => field;
            set => SetProperty<RandomActiveContentViewModel>(ref field, value);
        }

       // private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => field;
            set
            {
                if (SetProperty<bool>(ref field, value))
                {
                    _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Publish(new() { IsPaneOpen = field });
                }
            }
        }
        #endregion

        #region Register SignIned Event
        private void RegisterSignInedEvent()
        {
            _eventAggregator.GetEvent<OnSignInedEvent>().Subscribe((siet) =>
            {
                if (siet.IsSuccessful) 
                {
                    //var hamburgerMenuSideBarHubView = ShellContentActiveContentViewModel.GetStoreViewElementByName("HamburgerMenuSideBarHubView") as HamburgerMenuSideBarHubView;
                    //var hamburgerMenuSideBarHubViewModel = hamburgerMenuSideBarHubView.DataContext as HamburgerMenuSideBarHubViewModel;
                    ShellContentActiveContentViewModel.SetContentItemByName("HamburgerMenuSideBarHubView");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register AccessTokenExpired Event
        private void RegisterAccessTokenExpiredEvent()
        {
            _eventAggregator.GetEvent<OnAccessTokenExpiredEvent>().Subscribe((atee) =>
            {
                if (atee.IsExpired)
                {
                    //ShellContentActiveContentViewModel.RetsetContentItem(new ContentInformation
                    //{
                    //    Name = "LoginView",
                    //    Title = "LoginView",
                    //    ViewName = "Aksl.Modules.Account.Views.LoginView,Aksl.Modules.Account"
                    //});
                    ShellContentActiveContentViewModel.RetsetContentItemByName("LoginView");

                    var loginStatusView = LoginActiveContentViewModel.GetStoreViewElementByName("LoginStatusView") as LoginStatusView;
                    var loginStatusViewModel = loginStatusView.DataContext as LoginStatusViewModel;
                    loginStatusViewModel.IsSignIning = true;
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Register ActiveContent Method
        private async Task RegisterActiveContents()
        {
            try
            {
                _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.ShellContent);
                ShellContentActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.ShellContent);

                RegisterShellContentActiveContent();
                void RegisterShellContentActiveContent()
                {
                    ShellContentActiveContentViewModel.Add(new()
                    {
                        Name = "LoginView",
                        Title = "LoginView",
                        ViewName = "Aksl.Modules.Account.Views.LoginView,Aksl.Modules.Account",
                        //ViewElement = new LoginView(),
                    }, true);

                    ShellContentActiveContentViewModel.Add(new()
                    {
                        Name = "HamburgerMenuSideBarHubView",
                        Title = "HamburgerMenuSideBarHubView",
                        ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarHubView,Aksl.Modules.HamburgerMenuSideBar",
                        //ViewElement = new HamburgerMenuSideBarHubView()
                    }, false);

                    //ShellContentActiveContentViewModel.Add(new()
                    //{
                    //    Name = "HamburgerMenuNavigationSideBarHubView",
                    //    Title = "NavigationSideBarHubView",
                    //    ViewName = "Aksl.Modules.HamburgerMenuNavigationSideBar.Views.HamburgerMenuNavigationSideBarHubView,Aksl.Modules.HamburgerMenuNavigationSideBar",
                    //    //ViewElement = new HamburgerMenuSideBarHubView()
                    //}, false);
                }

                RegisterLoginActiveContent();
                void RegisterLoginActiveContent()
                {
                    _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.LoginContent);
                    LoginActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.LoginContent);
                 
                    LoginActiveContentViewModel.Add(new()
                    {
                        Name = "LoginStatusView",
                        Title = "LoginStatusView",
                        ViewName = "Aksl.Modules.Account.Views.LoginStatusView,Aksl.Modules.Account",
                        // ViewElement = new LoginStatusView(),
                    },true);
                }
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Registering Message \"{ex.Message}\"", title: "Error:Register ActiveContents");
            }
        }
        #endregion

        #region Register ContentChanged Event
        private void RegisterContentChangedEvents()
        {
            _eventAggregator.GetEvent<OnContentChangedViewEvent>().Subscribe(async (ccve) =>
            {
                try
                {
                    string viewTypeAssemblyQualifiedName = ccve.CurrentMenuItem.ViewName;
                    Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
                    // var view = _container.Resolve(viewType);
                    if (viewType is not null)
                    {
                        IRegion region = _regionManager.Regions[RegionNames.ShellContentRegion];
                        var viewName = viewType.Name;

                        _currentView = region.Views.FirstOrDefault(v => v.GetType() == viewType);
                        if (_currentView is null)
                        {
                            _currentView = region.GetView(viewType.FullName);
                        }

                        if (_currentView is not null)
                        {
                            if (ccve.CurrentMenuItem.IsCacheable)
                            {
                                region.Activate(_currentView);
                            }
                            else
                            {
                                region.Remove(_currentView);

                                AddView();
                            }
                        }
                        else
                        {
                            AddView();
                        }

                        void AddView()
                        {
                            if (IsSelectedOnInitialize())
                            {
                                _regionManager.RequestNavigate(RegionNames.ShellContentRegion, viewName);
                            }
                            else if (CanAddView())
                            {
                                NavigationParameters navigationParameters = new()
                                {
                                    { "CurrentMenuItem", ccve.CurrentMenuItem }
                                };

                                _regionManager.RequestNavigate(RegionNames.ShellContentRegion, viewName, navigationParameters);
                            }
                            else
                            {
                                NavigationParameters navigationParameters = new()
                                {
                                    { "CurrentMenuItem", ccve.CurrentMenuItem }
                                };

                                _regionManager.RequestNavigate(RegionNames.ShellContentRegion, viewName, navigationParameters);
                            }
                        }

                        bool IsSelectedOnInitialize() => !string.IsNullOrEmpty(ccve.CurrentMenuItem.ModuleName) && ccve.CurrentMenuItem.IsSelectedOnInitialize;

                        bool CanAddView() => !string.IsNullOrEmpty(ccve.CurrentMenuItem.ModuleName) && ccve.CurrentMenuItem.SubMenus.Count == 0;
                    }
                    else
                    {
                        await _dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
                    }
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Unable to loading \"{ccve.CurrentMenuItem.ModuleName}\" module.: \"{ex.Message}\"", title: "Error: Load Module");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion
    }
}
