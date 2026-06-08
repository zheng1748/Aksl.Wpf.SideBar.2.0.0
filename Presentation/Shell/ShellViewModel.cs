using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

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

            RegisterActiveContents().Await(configureAwait:true);
        }
        #endregion

        #region Properties
        private ActiveContentViewModel _shellContentActiveContentViewModel;
        public ActiveContentViewModel ShellContentActiveContentViewModel
        {
            get => _shellContentActiveContentViewModel;
            set => SetProperty<ActiveContentViewModel>(ref _shellContentActiveContentViewModel, value);
        }

        public ActiveContentViewModel LoginActiveContentViewModel { get; set; }

        private bool _isPaneOpen = true;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                if (SetProperty<bool>(ref _isPaneOpen, value))
                {
                    _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Publish(new() { IsPaneOpen = _isPaneOpen });
                }
            }
        }
        #endregion

        #region Register ActiveContent Method
        private async Task RegisterActiveContents()
        {
            try
            {
                RegisterShellActiveContent();
                void RegisterShellActiveContent()
                {
                    _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.ShellContent);
                    ShellContentActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<ActiveContentViewModel>(name: ActiveContentNames.ShellContent);

                    RegisterShellContentActiveContent();
                    void RegisterShellContentActiveContent()
                    {
                        ShellContentActiveContentViewModel.Add(new()
                        {
                            Name = "LoginView",
                            Title = "LoginView",
                            ViewName = "Aksl.Modules.Account.Views.LoginView,Aksl.Modules.Account",
                            //ViewElement = new LoginView(),
                        }, false);

                        ShellContentActiveContentViewModel.Add(new()
                        {
                            Name = "HamburgerMenuSideBarHubView",
                            Title = "HamburgerMenuSideBarHubView",
                            ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarHubView,Aksl.Modules.HamburgerMenuSideBar",
                            //ViewElement = new HamburgerMenuSideBarHubView()
                        }, true);

                        ShellContentActiveContentViewModel.Add(new()
                        {
                            Name = "HamburgerMenuNavigationSideBarHubView",
                            Title = "NavigationSideBarHubView",
                            ViewName = "Aksl.Modules.HamburgerMenuNavigationSideBar.Views.HamburgerMenuNavigationSideBarHubView,Aksl.Modules.HamburgerMenuNavigationSideBar",
                            //ViewElement = new HamburgerMenuSideBarHubView()
                        }, false);
                    }

                    RegisterLoginActiveContent();
                    void RegisterLoginActiveContent()
                    {
                        _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.LoginContent);
                        LoginActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<ActiveContentViewModel>(name: ActiveContentNames.LoginContent);

                        LoginActiveContentViewModel.Add(new()
                        {
                            Name = "LoginStatusView",
                            Title = "LoginStatusView",
                            ViewName = "Aksl.Modules.Account.Views.LoginStatusView,Aksl.Modules.Account",
                            // ViewElement = new LoginStatusView(),
                        });
                    }
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
