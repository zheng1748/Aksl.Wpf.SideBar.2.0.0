using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.Account.Views;
using Aksl.Modules.HamburgerMenuSideBar.Views;
using Microsoft.Extensions.DependencyInjection;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
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
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            _serviceProvider = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IServiceProvider>();
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            RegisterActiveContents();

            RegisterContentChangedEvents();
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
        private void RegisterActiveContents()
        {
            RegisterShellContentActiveContent();
            void RegisterShellContentActiveContent()
            {
                _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.ShellContent);
                var shellContentActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.ShellContent);

                shellContentActiveContentViewModel.Add(new()
                {
                    Name = nameof(LoginView),
                    Title = nameof(LoginView),
                    ViewName = "",
                    ViewElement = new LoginView(),
                }, false);

                shellContentActiveContentViewModel.Add(new()
                {
                    Name = nameof(HamburgerMenuSideBarHubView),
                    Title = nameof(HamburgerMenuSideBarHubView),
                    ViewName = "",
                    ViewElement = new HamburgerMenuSideBarHubView()
                });

                ShellContentActiveContentViewModel = shellContentActiveContentViewModel;
            }

            RegisterLoginActiveContent();
            void RegisterLoginActiveContent()
            {
                _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.LoginContent);
                var loginActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.LoginContent);

                loginActiveContentViewModel.Add(new()
                {
                    Name = nameof(LoginStatusView),
                    Title = nameof(LoginStatusView),
                    ViewName = "",
                    ViewElement = new LoginStatusView(),
                });

                LoginActiveContentViewModel = loginActiveContentViewModel;
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
