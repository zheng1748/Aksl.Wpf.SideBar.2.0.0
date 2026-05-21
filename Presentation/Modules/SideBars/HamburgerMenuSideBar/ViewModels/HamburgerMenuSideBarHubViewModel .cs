using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuSideBar.Views;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBar.ViewModels
{
    public class HamburgerMenuSideBarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
        //private object _currentView;
        private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarHubViewModel()
        {
            _container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            _regionManager = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionManager>();
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();

            _menuService = _container.Resolve<IMenuService>();

            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            IsPaneOpen = true;
            SelectedPlacement = SplitViewPanePlacement.Left;

            _workspaceViewEventName = "OnBuildHamburgerMenuSideBarWorkspaceViewEvent";
            WorkspaceRegionName = RegionNames.HamburgerMenuSideBarWorkspaceRegion;

            CreateHamburgerMenuSideBarViewModelAsync().Await();

            RegisterActiveContents();
            // RegisterBuildWorkspaceViewEvents();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public HamburgerMenuSideBarViewModel HamburgerMenuSideBar { get; set; }

        private HamburgerMenuSideBarItemViewModel _selectedHamburgerMenuSideBarItem;
        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get => _selectedHamburgerMenuSideBarItem;
            set
            {
                if (SetProperty(ref _selectedHamburgerMenuSideBarItem, value))
                {
                    if (_selectedHamburgerMenuSideBarItem is not null && !_selectedHamburgerMenuSideBarItem.HasSubMenu)
                    {
                        LoadViewAsync(_selectedHamburgerMenuSideBarItem.MenuItem).Await();
                    }
                }
            }
        }

        public ActiveContentViewModel LeftPaneActiveContentViewModel { get; set; }
        public ActiveContentViewModel RightContentActiveContentViewModel { get; set; }

        private string _workspaceRegionName;
        public string WorkspaceRegionName
        {
            get => _workspaceRegionName;
            set => SetProperty<string>(ref _workspaceRegionName, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }
        #endregion

        #region HamburgerMenu Properties
        private Brush _paneBackground = new SolidColorBrush(Colors.White);
        public Brush PaneBackground
        {
            get => _paneBackground;
            set => SetProperty<Brush>(ref _paneBackground, value);
        }

        public GridLength OpenPaneGridLength
        {
            get { return new GridLength(OpenPaneLength); }
        }

        private double _openPaneLength = 320d;
        public double OpenPaneLength
        {
            get => _openPaneLength;
            set => SetProperty<double>(ref _openPaneLength, value);
        }

        public GridLength CompactPaneGridLength
        {
            get { return new GridLength(CompactPaneLength); }
        }

        private double _compactPaneLength = 48d;
        public double CompactPaneLength
        {
            get => _compactPaneLength;
            set => SetProperty<double>(ref _compactPaneLength, value);
        }

        private bool _isPaneOpen = false;
        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                if (SetProperty<bool>(ref _isPaneOpen, value))
                {
                    if (HamburgerMenuSideBar is not null)
                    {
                        HamburgerMenuSideBar.IsPaneOpen = value;
                    }

                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewDisplayMode> DisplayModeList
        {
            get => Enum.GetValues(typeof(SplitViewDisplayMode)).Cast<SplitViewDisplayMode>().ToList();
        }

        private SplitViewDisplayMode _selectedDisplayMode = SplitViewDisplayMode.Overlay;
        public SplitViewDisplayMode SelectedDisplayMode
        {
            get => _selectedDisplayMode;
            set
            {
                if (SetProperty<SplitViewDisplayMode>(ref _selectedDisplayMode, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        public List<SplitViewPanePlacement> PanePlacementList
        {
            get => Enum.GetValues(typeof(SplitViewPanePlacement)).Cast<SplitViewPanePlacement>().ToList();
        }

        private SplitViewPanePlacement _selectedPanePlacement = SplitViewPanePlacement.Left;
        public SplitViewPanePlacement SelectedPlacement
        {
            get => _selectedPanePlacement;
            set
            {
                if (SetProperty<SplitViewPanePlacement>(ref _selectedPanePlacement, value))
                {
                    VisualState = GetVisualState();
                }
            }
        }

        private string _visualState;
        public string VisualState
        {
            get => _visualState;
            set => SetProperty<string>(ref _visualState, value);
        }

        private bool _isLoading;

        #endregion

        #region Get HamburgerMenu State Method
        private bool IsCompact
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.CompactOverlay => true,
                    _ => false,
                };
            }
        }

        private bool IsInline
        {
            get
            {
                return SelectedDisplayMode switch
                {
                    SplitViewDisplayMode.CompactInline or SplitViewDisplayMode.Inline => true,
                    _ => false
                };
            }
        }

        protected virtual string GetVisualState()
        {
            string state;

            if (IsPaneOpen)
            {
                state = "Open";
                state += IsInline ? "Inline" : SelectedDisplayMode.ToString();
            }
            else
            {
                state = "Closed";
                if (IsCompact)
                {
                    state += "Compact";
                }
                //else
                //{
                //    return state;
                //}
            }

            state += SelectedPlacement.ToString();

            return state;
        }
        #endregion

        #region Register ActiveContents Method
        private void RegisterActiveContents()
        {
            RegisterLeftPaneActiveContent();
            void RegisterLeftPaneActiveContent()
            {
                _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
                var leftPaneActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

                leftPaneActiveContentViewModel.Add(new()
                {
                    Name = nameof(HamburgerMenuSideBarView),
                    Title = nameof(HamburgerMenuSideBarView),
                    ViewName = "",
                    ViewElement = new HamburgerMenuSideBarView() { DataContext= HamburgerMenuSideBar },
                });

                LeftPaneActiveContentViewModel = leftPaneActiveContentViewModel;
            }

            RegisterRightContentActiveContent();
            void RegisterRightContentActiveContent()
            {
                _container.RegisterSingleton(from: typeof(ActiveContentViewModel), to: typeof(ActiveContentViewModel), name: ActiveContentNames.RightContentHamburgerMenuSideBar);
                var rightContentActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuSideBar);

                RightContentActiveContentViewModel = rightContentActiveContentViewModel;
            }
        }
        #endregion

        #region Register BuildWorkspaceView Event
        private void RegisterBuildWorkspaceViewEvents()
        {
            var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(_workspaceViewEventName) as OnBuildWorkspaceViewEventbase;
            Debug.Assert(buildHWorkspaceViewEvent is not null);

            // _eventAggregator.GetEvent<OnBuildHamburgerMenuSideBarWorkspaceViewEvent>().Subscribe(async (bhmsbwve) =>
            buildHWorkspaceViewEvent.Subscribe(async (bmve) =>
            {
                var currentMenuItem = bmve.CurrentMenuItem;
                var currentActiveContent = RightContentActiveContentViewModel;

                try
                {
                    await LoadViewAsync();

                    #region LoadView Method
                    async Task LoadViewAsync()
                    {
                        string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
                        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
                        if (viewType is not null)
                        {
                            //IRegion region = _regionManager.Regions[WorkspaceRegionName];
                            var viewName = viewType.Name;

                            ContentInformation contentInformation = new()
                            {
                                Name = currentMenuItem.Name,
                                Title = currentMenuItem.Title,
                                ViewName = currentMenuItem.ViewName
                            };

                            //_currentView = region.GetView(viewTypeAssemblyQualifiedName);
                            //var currentView = region.Views.FirstOrDefault(v => v.GetType() == viewType);
                            var currentView = currentActiveContent.GetStoreViewElementByName(currentMenuItem.Name);
                            if (currentView is null)
                            {
                               // currentView = region.GetView(viewType.FullName);
                            }

                            if (currentView is not null)
                            {
                                if (currentMenuItem.IsCacheable)
                                {
                                    // region.Activate(currentView);
                                    currentActiveContent.SetContentItem(contentInformation);
                                }
                                else
                                {
                                    //region.Remove(currentView);
                                    currentActiveContent.RetsetContentItem(contentInformation);
                                   // AddView();
                                }
                            }
                            else
                            {
                                AddView();
                            }

                            void AddView()
                            {
                                if (CanAddView())
                                {
                                    currentActiveContent.Add(contentInformation);

                                    //NavigationParameters navigationParameters = new()
                                    //{
                                    //    { "CurrentMenuItem", currentMenuItem }
                                    //};

                                    //_regionManager.RequestNavigate(WorkspaceRegionName, viewName, navigationParameters);
                                }
                            }

                            bool CanAddView() => !string.IsNullOrEmpty(currentMenuItem.ModuleName);
                        }
                        else
                        {
                            await _dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
                        }
                    }
                    #endregion

                    #region Navigated To LoginView Method
                    //void NavigatedToLoginView()
                    //{
                    //    var contentRegion = _regionManager.Regions[RegionNames.ShellContentRegion];
                    //    var activeViews = contentRegion.ActiveViews;
                    //    if (activeViews is not null && activeViews.Any())
                    //    {
                    //        _activeView = activeViews.FirstOrDefault();
                    //    }

                    //    var viewName = GetViewName();

                    //    var loginViewName = "LoginView";
                    //    (string ViewName, Infrastructure.MenuItem CurrentMenuItem, string LoginViewName, object ActiveView, object SelectedHamburgerMenuItem, object PreviewSelectedHamburgerMenuItem) parameters = (viewName, currentMenuItem, loginViewName, _activeView, selectedHamburgerMenuItem, previewSelectedHamburgerMenuItem);

                    //    NavigationParameters navigationParameters = new()
                    //    {
                    //       {NavigationParameterNames.NavToSignIn,parameters},
                    //    };

                    //    _regionManager.RequestNavigate(RegionNames.ShellContentRegion, loginViewName, navigationParameters);
                    //}
                    #endregion

                    #region GetViewName Method
                    //string GetViewName()
                    //{
                    //    string viewName = default;

                    //    string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
                    //    Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
                    //    if (viewType is not null)
                    //    {
                    //        viewName = viewType.Name;
                    //    }

                    //    return viewName;
                    //}
                    #endregion

                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Unable to loading \"{currentMenuItem.ModuleName}\" module.: \"{ex.Message}\"", title: "Error: Load Module");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region LoadView Method
        private async Task LoadViewAsync(MenuItem currentMenuItem)
        {
            var currentActiveContent = RightContentActiveContentViewModel;

           string viewTypeAssemblyQualifiedName = currentMenuItem.ViewName;
            Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
            if (viewType is not null)
            {
                var viewName = viewType.Name;

                ContentInformation contentInformation = new()
                {
                    Name = currentMenuItem.Name,
                    Title = currentMenuItem.Title,
                    ViewName = currentMenuItem.ViewName
                };
                ;
                var currentView = currentActiveContent.GetStoreViewElementByName(currentMenuItem.Name);

                if (currentView is not null)
                {
                    if (currentMenuItem.IsCacheable)
                    {
                        currentActiveContent.SetContentItem(contentInformation);
                    }
                    else
                    {
                        currentActiveContent.RetsetContentItem(contentInformation);
                    }
                }
                else
                {
                    currentActiveContent.Add(contentInformation);
                }
            }
            else
            {
                await _dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
            }
        }
        #endregion

        #region Register HamburgerMenuBarPaneOpen Event
        private void RegisterHamburgerMenuBarPaneOpenEvent()
        {
            _eventAggregator.GetEvent<OnHamburgerMenuBarPaneOpenEvent>().Subscribe(async (hmbpoe) =>
            {
                try
                {
                    IsPaneOpen = hmbpoe.IsPaneOpen;
                }
                catch (Exception ex)
                {
                    await _dialogViewService.AlertAsync(message: $"Subscribe PaneOpen Event Error.: \"{ex.Message}\"", title: "Error");
                }
            }, ThreadOption.UIThread, true);
        }
        #endregion

        #region Create HamburgerMenuSideBar ViewModel Method
        private async Task CreateHamburgerMenuSideBarViewModelAsync()
        {
            IsLoading = true;

            try
            {
                HamburgerMenuSideBar = new();
                AddPropertyChanged();

                void AddPropertyChanged()
                {
                    HamburgerMenuSideBar.PropertyChanged += (sender, e) =>
                    {
                        if (sender is HamburgerMenuSideBarViewModel hmbvm)
                        {
                            if (e.PropertyName == nameof(HamburgerMenuSideBarViewModel.IsLoading) && !hmbvm.IsLoading)
                            {
                                IsLoading = false;
                            }

                            if (e.PropertyName == nameof(HamburgerMenuSideBarViewModel.SelectedHamburgerMenuSideBarItem))
                            {
                                if (SelectedHamburgerMenuSideBarItem is null)
                                {
                                    SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;
                                }

                                if (SelectedHamburgerMenuSideBarItem is not null && SelectedHamburgerMenuSideBarItem != hmbvm.SelectedHamburgerMenuSideBarItem)
                                {
                                    SelectedHamburgerMenuSideBarItem = null;

                                    SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;
                                }
                            }
                        }
                    };
                }

                var hamburgerMenuBarItemViewModels = await CreateHamburgerMenuBarItemViewModelsAsync();
                HamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(hamburgerMenuBarItemViewModels);

                HamburgerMenuSideBar.WorkspaceViewEventName = _workspaceViewEventName;
                HamburgerMenuSideBar.SetWorkspaceViewEventName();

                //  await HamburgerMenuSideBar.CreateHamburgerMenuBarItemViewModelsAsync();
                HamburgerMenuSideBar.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(HamburgerMenuSideBar));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create hamburger menu : \"{ex.Message}\"", title: "Error: Create HamburgerMenu");
            }
            finally
            {
                if (IsLoading)
                {
                    IsLoading = false;
                }
            }
        }
        #endregion

        #region Create HamburgerMenuItemBar ViewModel Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> CreateHamburgerMenuBarItemViewModelsAsync()
        {
            List<HamburgerMenuSideBarItemViewModel> allLeafs = new();

            var rootMenuItem = await _menuService.GetMenuAsync("All");

            var subMenuItems = rootMenuItem.SubMenus;
            foreach (var smi in subMenuItems)
            {
                //var allLeafsOfMenuItems = await GetLeafsOfMenuItemAsync(smi);
               // allLeafs.AddRange(allLeafsOfMenuItems);

                //var allLeafsOfTopMenuItems = await GetLeafsOfTopMenuItemAsync(smi);
                //allLeafs.AddRange(allLeafsOfTopMenuItems);

                var topHeaderItem = await GetTopHeaderByMenuItemAsync(smi);
                var allLeafsOfTopHeaderItems = await GetLeafsOfTopHeaderItemAsync(topHeaderItem);
                allLeafs.AddRange(allLeafsOfTopHeaderItems);
            }

            var allDistinctLeafs = allLeafs.DistinctBy(item => (item.Name, item.Title)).ToList();
            allLeafs = new(allDistinctLeafs);

            return allLeafs;
        }
        #endregion

        #region Get Leafs Of MenuItem Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> GetLeafsOfMenuItemAsync(MenuItem menuItem)
        {
            List<MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem)
            {
                var isAddOnLeaf = IsLeaf(currentMenuItem) && (!HasNavigationName(currentMenuItem) || (HasNavigationName(currentMenuItem) && !IsNextNavigation(currentMenuItem)));
                var isAddOnNotLeaf = !IsLeaf(currentMenuItem) && !IsNexOnNotLeaf(currentMenuItem);
                if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem) && HasTitle(currentMenuItem) && (isAddOnLeaf || isAddOnNotLeaf))
                {
                    leafsOfMenuItem.Add(new(currentMenuItem, null));
                    travelMenuItems.Add(currentMenuItem);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

                if (HasSubMenu(currentMenuItem) && IsNexOnNotLeaf(currentMenuItem))
                {
                    foreach (var smi in currentMenuItem.SubMenus)
                    {
                        await RecursiveSubMenuItem(smi);
                    }
                }
            }

            bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafsOfMenuItem;
        }
        #endregion

        #region Get Leafs Of Top MenuItem Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> GetLeafsOfTopMenuItemAsync(MenuItem menuItem)
        {
            List<MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();
            HamburgerMenuSideBarItemViewModel virtualParent = new();

            await RecursiveSubMenuItem(menuItem, virtualParent);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem, HamburgerMenuSideBarItemViewModel paren)
            {
                HamburgerMenuSideBarItemViewModel child = default;

                if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem))
                {
                    travelMenuItems.Add(currentMenuItem);

                    child = new(currentMenuItem, paren);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

                if (HasSubMenu(currentMenuItem) && IsNexOnNotLeaf(currentMenuItem))
                {
                    foreach (var smi in currentMenuItem.SubMenus)
                    {
                        await RecursiveSubMenuItem(smi, child);
                    }
                }
            }

            var topHeaderItem = virtualParent.Children.FirstOrDefault() as HamburgerMenuSideBarItemViewModel;
            if (topHeaderItem is not null)
            {
                topHeaderItem.Parent = null;

                leafsOfMenuItem = await GetLeafsOfTopHeaderItemAsync(topHeaderItem);
            }

            bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafsOfMenuItem;
        }
        #endregion

        #region Get Top Header Method
        private async Task<HamburgerMenuSideBarItemViewModel> GetTopHeaderByMenuItemAsync(MenuItem menuItem)
        {
            List<MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();
            HamburgerMenuSideBarItemViewModel virtualParent = new();

            await RecursiveSubMenuItem(menuItem, virtualParent);

            async Task RecursiveSubMenuItem(MenuItem currentMenuItem, HamburgerMenuSideBarItemViewModel paren)
            {
                HamburgerMenuSideBarItemViewModel child = default;

                if (!AnyEqualsMenuItems(travelMenuItems, currentMenuItem))
                {
                    travelMenuItems.Add(currentMenuItem);

                    child = new(currentMenuItem, paren);
                }

                if (HasNavigationName(currentMenuItem) && IsNextNavigation(currentMenuItem))
                {
                    currentMenuItem = await _menuService.GetMenuAsync(currentMenuItem.NavigationName);
                }

                if (HasSubMenu(currentMenuItem) && IsNexOnNotLeaf(currentMenuItem))
                {
                    foreach (var smi in currentMenuItem.SubMenus)
                    {
                        await RecursiveSubMenuItem(smi, child);
                    }
                }
            }

            bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            var topHeaderItem = virtualParent.Children.FirstOrDefault();
            if (topHeaderItem is not null)
            {
                topHeaderItem.Parent = null;
            }
            return topHeaderItem as HamburgerMenuSideBarItemViewModel;
        }
        #endregion

        #region Get Leafs Of TopHeaderItem Method
        private async Task<List<HamburgerMenuSideBarItemViewModel>> GetLeafsOfTopHeaderItemAsync(HamburgerMenuSideBarItemViewModel topHeaderItem)
        {
            List<HamburgerMenuSideBarItemViewModel> leafsOfTopHeaderItem = new();

            await RecursiveSubMenuItemViewModel(topHeaderItem);

            async Task RecursiveSubMenuItemViewModel(HamburgerMenuSideBarItemViewModel currenySubItem)
            {
                if (!AnyEqualsHamburgerMenuSideBarItemViewModels(leafsOfTopHeaderItem, currenySubItem) && currenySubItem.IsLeaf && currenySubItem.HasTitle)
                {
                    leafsOfTopHeaderItem.Add(currenySubItem);
                }

                if (currenySubItem.HasChildren)
                {
                    foreach (var children in currenySubItem.Children)
                    {
                        await RecursiveSubMenuItemViewModel(children as HamburgerMenuSideBarItemViewModel);
                    }
                }
            }

            return leafsOfTopHeaderItem;
        }
        #endregion

        #region Contain Methods
        private bool AnyEqualsHamburgerMenuSideBarItemViewModels(IEnumerable<HamburgerMenuSideBarItemViewModel> hamburgerMenuSideBarItemViewModels, HamburgerMenuSideBarItemViewModel hamburgerMenuSideBarItemViewModel)
        {
            if (hamburgerMenuSideBarItemViewModels is null || (hamburgerMenuSideBarItemViewModels is not null && !hamburgerMenuSideBarItemViewModels.Any()) || hamburgerMenuSideBarItemViewModel is null)
            {
                return false;
            }

            var isAny = hamburgerMenuSideBarItemViewModels.Any(hmivm => IsEqualsNameOrTitle(hmivm.Name, hamburgerMenuSideBarItemViewModel.Name) || IsEqualsNameOrTitle(hmivm.Title, hamburgerMenuSideBarItemViewModel.Title));

            return isAny;
        }

        private bool AnyEqualsMenuItems(IEnumerable<MenuItem> menuItems, MenuItem menuItem)
        {
            var isAny = menuItems.Any(mi => IsEqualsNameOrTitle(mi.Name, menuItem.Name) || IsEqualsNameOrTitle(mi.Title, menuItem.Title));

            return isAny;
        }

        private bool IsEqualsNameOrTitle(string nameOrTitle, string otherNameOrTitle)
        {
            if (string.IsNullOrEmpty(nameOrTitle) || string.IsNullOrEmpty(otherNameOrTitle))
            {
                return false;
            }

            var isAny = (!string.IsNullOrEmpty(nameOrTitle) && nameOrTitle.Equals(otherNameOrTitle, StringComparison.InvariantCultureIgnoreCase)) ||
                        (!string.IsNullOrEmpty(otherNameOrTitle) && otherNameOrTitle.Equals(nameOrTitle, StringComparison.InvariantCultureIgnoreCase));

            return isAny;
        }
        #endregion

        #region INavigationAware
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var parameters = navigationContext.Parameters;
            if (parameters is not null)
            {
                if (parameters.Count == 0)
                {
                   // CreateHamburgerMenuSideBarViewModelAsync().GetAwaiter().GetResult();
                }
                //else
                //{
                //    if (isSignIning && parameters.TryGetValue(NavigationParameterNames.NavBackFromSignIn, out object navFromParameter))
                //    {
                //        (string UserName, bool IsSuccessful, Infrastructure.MenuItem CurrentMenuItem, object SelectedHamburgerMenuItem, object PreviewSelectedHamburgerMenuItem) fromParameter = ((string, bool, Infrastructure.MenuItem, object, object))navFromParameter;
                //        if (!fromParameter.IsSuccessful)
                //        {
                //            isSignIning = false;

                //            if (fromParameter.PreviewSelectedHamburgerMenuItem is not null)
                //            {
                //                var contentRegion = _regionManager.Regions[RegionNames.ShellContentRegion];
                //                var activeViews = contentRegion.Views;
                //                var count = activeViews.Count();
                //                HamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem = fromParameter.PreviewSelectedHamburgerMenuItem as HamburgerMenuSideBarItemViewModel;
                //            }
                //            else
                //            {
                //                HamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem = null;
                //            }
                //        }
                //        else if (fromParameter.IsSuccessful)
                //        {
                //            isSignIning = false;
                //            await LoadViewAsync(fromParameter.CurrentMenuItem);
                //        }
                //    }
                //}
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
