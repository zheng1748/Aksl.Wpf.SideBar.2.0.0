using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace Aksl.Modules.MenuSub.ViewModels
{
    public class MenuSubHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        private object _currentView; 
        private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public MenuSubHubViewModel()
        {
            _container = PrismIocExtensions.GetContainer();
            _regionManager = PrismUnityExtensions.GetRegionManager();
            _eventAggregator = PrismUnityExtensions.GetEventAggregator();

            _dialogViewService = PrismUnityExtensions.GetDialogViewService();
            _menuService = PrismUnityExtensions.GetMenuService();
        }
        #endregion

        #region Properties
        //private string _workspaceRegionName;
        //public string WorkspaceRegionName
        //{
        //    get => _workspaceRegionName;
        //    set => SetProperty<string>(ref _workspaceRegionName, value);
        //}
        public string ActiveContentName { get; set; }
        private RandomActiveContentViewModel _randomActiveContentViewModel;
        public RandomActiveContentViewModel BottomActiveContentViewModel
        {
            get => _randomActiveContentViewModel;
            set => SetProperty<RandomActiveContentViewModel>(ref _randomActiveContentViewModel, value);
        }
        public HierarchicalMenusViewModel HierarchicalMenus { get; private set; }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        #endregion

        #region Register ActiveContent Method
        private void RegisterActiveContent()
        {
            _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: this.ActiveContentName);
            var bottomActiveContentViewModel = _container.Resolve<RandomActiveContentViewModel>(name: this.ActiveContentName);

            BottomActiveContentViewModel = bottomActiveContentViewModel;
        }
        #endregion

        #region Register BuildWorkspaceView Event
        private void RegisterBuildWorkspaceViewEvents()
        {
            var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(_workspaceViewEventName) as OnBuildWorkspaceViewEventbase;
            Debug.Assert(buildHWorkspaceViewEvent is not null);

            buildHWorkspaceViewEvent.Subscribe(async (bmve) =>
            {
                var currentMenuItem = bmve.CurrentMenuItem;

                try
                {
                    await LoadViewAsync();

                    #region LoadView Method
                    async Task LoadViewAsync()
                    {
                        if (IsAddViewToBottomContent())
                        {
                            AddViewToBottomContent();
                        }

                        if (IsNavigationToBottomContent())
                        {
                            NavigationToBottomContent();
                        }
                    }

                    void AddViewToBottomContent()
                    {
                        ActiveContentManagerExtensions.AddViewToRandomContentAsync(currentMenuItem, this.ActiveContentName).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
                        {
                            System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                            {
                                await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To BottomContent");
                            });
                        });
                    }

                    void NavigationToBottomContent()
                    {
                        ActiveContentManagerExtensions.NavigationToRandomContentAsync(currentMenuItem, this.ActiveContentName, new() { { "CurrentMenuItem", currentMenuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
                        {
                            System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                            {
                                await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To RightContent");
                            });
                        });
                    }

                    bool IsAddViewToBottomContent() =>
                                        !currentMenuItem.HasNextSubMenu() && currentMenuItem.HasViewName() && !currentMenuItem.IsNexApplication;

                    bool IsNavigationToBottomContent() =>
                            currentMenuItem.HasNextSubMenu() && currentMenuItem.HasViewName() && currentMenuItem.IsNexApplication;
                    #endregion
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Unable to loading \"{currentMenuItem.ModuleName}\" module.: \"{ex.Message}\"", title: "Error: Load Module");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Create HierarchicalMenus ViewModel Method
        private async Task CreateHierarchicalMenusViewModel(MenuItem currentMenuItem)
        {
            IsLoading = true;

            HierarchicalMenus = new(_eventAggregator, _menuService, currentMenuItem);
            AddPropertyChanged();

            void AddPropertyChanged()
            {
                HierarchicalMenus.PropertyChanged += (sender, e) =>
                {
                    if (sender is HierarchicalMenusViewModel hmvm)
                    {
                        if (e.PropertyName == nameof(HierarchicalMenusViewModel.IsLoading) && !hmvm.IsLoading)
                        {
                            IsLoading = false;
                        }
                    }
                };
            }

            await HierarchicalMenus.CreateHierarchicalMenuItemViewModelsAsync();
            RaisePropertyChanged(nameof(HierarchicalMenus));
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters.TryGetValue("CurrentMenuItem", out MenuItem currentMenuItem))
            {
              //  WorkspaceRegionName = currentMenuItem.WorkspaceRegionName;
                _workspaceViewEventName = currentMenuItem.WorkspaceViewEventName;

               ActiveContentName = currentMenuItem.ActiveContentName;
               RegisterActiveContent();

                RegisterBuildWorkspaceViewEvents();

                CreateHierarchicalMenusViewModel(currentMenuItem).GetAwaiter().GetResult();
            }
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
