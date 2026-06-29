using Aksl.ActiveContents.ViewModels;
using Aksl.ActiveContents.Views;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuSideBar.Views;
using Prism;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBar.ViewModels
{
    public class HamburgerMenuSideBarHubViewModel : BindableBase, INavigationAware
    {
        #region Members
        private readonly IUnityContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogViewService _dialogViewService;
        private readonly IMenuService _menuService;
       // private string _workspaceViewEventName;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarHubViewModel()
        {
            _container = PrismIocExtensions.GetContainer().Resolve<IUnityContainer>();
            _eventAggregator = PrismIocExtensions.GetContainer().Resolve<IEventAggregator>();
            _dialogViewService = PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();

            _menuService = _container.Resolve<IMenuService>();

            SelectedDisplayMode = SplitViewDisplayMode.CompactInline;
            IsPaneOpen = true;
            SelectedPlacement = SplitViewPanePlacement.Left;

            CreateMovePreviousCommand();

            RegisterPropertyChanged();
            RegisterActiveContentAsync().Await();
            RegisterHamburgerMenuBarPaneOpenEvent();
        }
        #endregion

        #region Properties
        public SequenceActiveContentViewModel LeftPaneActiveContentViewModel { get; set; }

        public RandomActiveContentViewModel RightContentActiveContentViewModel { get; set; }
        public HamburgerMenuSideBarViewModel TopHamburgerMenuSideBar { get; set; }

       // private ActiveContentItemViewModel _selectedLeftPaneActiveContentItem;
        public ActiveContentItemViewModel SelectedLeftPaneActiveContentItem
        {
            get => field;
            set => SetProperty(ref field, value);
        }

        private HamburgerMenuSideBarViewModel _selectedHamburgerMenuSideBar;
        public HamburgerMenuSideBarViewModel SelectedHamburgerMenuSideBar
        {
            get => _selectedHamburgerMenuSideBar;
            set => SetProperty(ref _selectedHamburgerMenuSideBar, value);
        }

        private HamburgerMenuSideBarItemViewModel _selectedHamburgerMenuSideBarItem;
        public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
        {
            get => _selectedHamburgerMenuSideBarItem;
            set => SetProperty(ref _selectedHamburgerMenuSideBarItem, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public bool CanMove
        {
            get
            {
                return LeftPaneActiveContentViewModel.CanMove;
            }
        }

        private Visibility _moveButtonVisibility = Visibility.Visible;
        public Visibility MoveButtonVisibility
        {
            get
            {
                var isAllAddView =  TopHamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems.All(msi => msi.IsLeaf && !msi.HasSubMenu && msi.HasViewName);
                _moveButtonVisibility = isAllAddView ? Visibility.Collapsed : Visibility.Visible;
                return _moveButtonVisibility;
            }
        }
        #endregion

        #region RegisterPropertyChanged Method
        private void RegisterPropertyChanged()
        {
            this.PropertyChanged += (sender, e) =>
            {
                if (sender is HamburgerMenuSideBarHubViewModel hmsbhvm)
                {
                    if (e.PropertyName == nameof(IsLoading))
                    {
                        (MovePreviousCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                    }
                }
            };
        }

        private void AddHamburgerMenuSideBarViewModelPropertyChanged(HamburgerMenuSideBarViewModel hamburgerMenuSideBarViewModel)
        {
            hamburgerMenuSideBarViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is HamburgerMenuSideBarViewModel hmbvm)
                {
                    //if (e.PropertyName == nameof(HamburgerMenuSideBarViewModel.IsLoading) && !hmbvm.IsLoading)
                    //{
                    //    IsLoading = false;
                    //}

                    if (e.PropertyName == nameof(HamburgerMenuSideBarViewModel.SelectedHamburgerMenuSideBarItem))
                    {
                        if (SelectedHamburgerMenuSideBar == hmbvm && SelectedHamburgerMenuSideBarItem is null && hmbvm.SelectedHamburgerMenuSideBarItem is not null)
                        {
                            if (IsAddViewToRightContent(hmbvm.SelectedHamburgerMenuSideBarItem))
                            {
                                hmbvm.LastHamburgerMenuSideBarItemWithNotSubMenu = hmbvm.SelectedHamburgerMenuSideBarItem;
                                SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;

                               // HamburgerMenuSideBarHelper.AddViewToRightContentAsync(hmbvm.SelectedHamburgerMenuSideBarItem.MenuItem).Await();
                            }

                            if (IsSetActiveToLeftPaneActiveContent(hmbvm.SelectedHamburgerMenuSideBarItem))
                            {
                                SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;

                                LeftPaneActiveContentViewModel.SetSelectedItemByName(hmbvm.SelectedHamburgerMenuSideBarItem.Path);
                            }
                        }

                        if (SelectedHamburgerMenuSideBar == hmbvm &&
                           (SelectedHamburgerMenuSideBarItem is not null && hmbvm.SelectedHamburgerMenuSideBarItem is not null && SelectedHamburgerMenuSideBarItem != hmbvm.SelectedHamburgerMenuSideBarItem))
                        {
                            if (IsAddViewToRightContent(hmbvm.SelectedHamburgerMenuSideBarItem))
                            {
                                hmbvm.LastHamburgerMenuSideBarItemWithNotSubMenu = hmbvm.SelectedHamburgerMenuSideBarItem;
                                SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;

                               // HamburgerMenuSideBarHelper.AddViewToRightContentAsync(hmbvm.SelectedHamburgerMenuSideBarItem.MenuItem).Await();
                            }

                            if (IsSetActiveToLeftPaneActiveContent(hmbvm.SelectedHamburgerMenuSideBarItem))
                            {
                                SelectedHamburgerMenuSideBarItem = hmbvm.SelectedHamburgerMenuSideBarItem;

                                LeftPaneActiveContentViewModel.SetSelectedItemByName(hmbvm.SelectedHamburgerMenuSideBarItem.Path);
                            }
                        }

                        bool IsAddViewToRightContent(HamburgerMenuSideBarItemViewModel menuSideBarItem)
                        {
                            return !menuSideBarItem.HasSubMenu && menuSideBarItem.IsLeaf && menuSideBarItem.IsSelected;
                        }

                        bool IsSetActiveToLeftPaneActiveContent(HamburgerMenuSideBarItemViewModel menuSideBarItem)
                        {
                            return menuSideBarItem.HasSubMenu && menuSideBarItem.IsSelected;
                        }
                    }
                }
            };
        }

        private void AddLeftPaneActiveContentViewModelPropertyChanged()
        {
            LeftPaneActiveContentViewModel.PropertyChanged += (sender, e) =>
            {
                if (sender is SequenceActiveContentViewModel savm)
                {
                    if (e.PropertyName == nameof(SequenceActiveContentViewModel.SelectedContentItem))
                    {
                        if (SelectedLeftPaneActiveContentItem is null)
                        {
                            SelectedLeftPaneActiveContentItem = savm.SelectedContentItem;

                            SetMenuSideBar();
                        }

                        if (SelectedLeftPaneActiveContentItem is not null && SelectedLeftPaneActiveContentItem != savm.SelectedContentItem)
                        {
                            SelectedLeftPaneActiveContentItem = null;
                            SelectedLeftPaneActiveContentItem = savm.SelectedContentItem;

                            SetMenuSideBar();
                        }
                    }

                    if (e.PropertyName == nameof(ActiveContentViewModel.SelectedIndex))
                    {
                        RaisePropertyChanged(nameof(CanMove));
                        (MovePreviousCommand as DelegateCommand<ActiveContentItemViewModel>)?.RaiseCanExecuteChanged();
                    }

                    void SetMenuSideBar()
                    {
                        var hamburgerMenuSideBarView = savm.SelectedContentItem.ViewElement as HamburgerMenuSideBarView;
                        var hamburgerMenuSideBarViewModel = hamburgerMenuSideBarView.DataContext as HamburgerMenuSideBarViewModel;

                        if (SelectedHamburgerMenuSideBar != hamburgerMenuSideBarViewModel)
                        {
                            SelectedHamburgerMenuSideBar = null;
                            SelectedHamburgerMenuSideBar = hamburgerMenuSideBarViewModel;

                            if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is null)
                            {
                                RightContentActiveContentViewModel.ClearSelectedItem();
                            }

                            //if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null && IsSetLeftPaneActiveContentItem(SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem))
                            if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null &&
                                SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.IsSetLeftPaneActiveContentItem)
                            {
                                if (hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu is not null &&
                                    hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu != SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem)
                                {
                                    // SelectedHamburgerMenuSideBarItem = hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu;

                                    hamburgerMenuSideBarViewModel.LastHamburgerMenuSideBarItemWithNotSubMenu.IsSelected = true;
                                }
                                else
                                {
                                    RightContentActiveContentViewModel.ClearSelectedItem();
                                }
                            }
                            else
                            {
                                //if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null && IsAddViewToRightContent(SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem))
                                if (SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem is not null &&
                                    SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.IsAddViewToRightContent)
                                {
                                    ActiveContentManagerExtensions.AddViewToRandomContentAsync(SelectedHamburgerMenuSideBar.SelectedHamburgerMenuSideBarItem.MenuItem, ActiveContentNames.RightContentHamburgerMenuSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
                                    {
                                        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                                        {
                                            await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View");
                                        });
                                    });
                                }
                            }
                        }
                    }

                    bool IsAddViewToRightContent(HamburgerMenuSideBarItemViewModel menuSideBarItem)
                    {
                        return !menuSideBarItem.HasSubMenu && menuSideBarItem.IsLeaf && menuSideBarItem.IsSelected;
                    }

                    bool IsSetLeftPaneActiveContentItem(HamburgerMenuSideBarItemViewModel menuSideBarItem)
                    {
                        return menuSideBarItem.HasSubMenu && menuSideBarItem.IsSelected;
                    }
                }
            };
        }
        #endregion

        #region HamburgerMenu Properties
      //  private Brush _paneBackground = new SolidColorBrush(Colors.Transparent);
        private Brush _paneBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D3D3D3"));
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
                    if (TopHamburgerMenuSideBar is not null)
                    {
                        TopHamburgerMenuSideBar.IsPaneOpen = value;
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

        #region Register ActiveContent Method
        private async Task RegisterActiveContentAsync()
        {
            RegisterRightContentActiveContent();
            void RegisterRightContentActiveContent()
            {
                _container.RegisterSingleton(from: typeof(RandomActiveContentViewModel), to: typeof(RandomActiveContentViewModel), name: ActiveContentNames.RightContentHamburgerMenuSideBar);
                var rightContentActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<RandomActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuSideBar);

                RightContentActiveContentViewModel = rightContentActiveContentViewModel;
            }

            await RegisterLeftPaneActiveContentAsync();
            async Task RegisterLeftPaneActiveContentAsync()
            {
                _container.RegisterSingleton(from: typeof(SequenceActiveContentViewModel), to: typeof(SequenceActiveContentViewModel), name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
                LeftPaneActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

                AddLeftPaneActiveContentViewModelPropertyChanged();

                CreateTopHamburgerMenuSideBarViewModelAsync().Await();
                LeftPaneActiveContentViewModel.Add(new()
                {
                    Name = "Root",
                    Title = "Root",
                    ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                    ViewElement = new HamburgerMenuSideBarView() { DataContext = TopHamburgerMenuSideBar }
                }, true);

               // var subMenuSideBas = await GetAllSubMenuSideBarViewModelsAsync(this.TopHamburgerMenuSideBar);
                //foreach (var msb in subMenuSideBas)
                //{
                //    AddHamburgerMenuSideBarViewModelPropertyChanged(msb.MenuSideBar);

                //    ContentInformation contentInfo = new()
                //    {
                //        Name = msb.Path,
                //        Title = msb.Path,
                //        ViewName = "",
                //        ViewElement = new HamburgerMenuSideBarView() { DataContext = msb.MenuSideBar }
                //    };

                //    LeftPaneActiveContentViewModel.Add(contentInfo, false);
                //}
            }
        }
        #endregion

        #region MovePrevious Command
        public ICommand MovePreviousCommand { get; set; }

        private void CreateMovePreviousCommand()
        {
            MovePreviousCommand = new DelegateCommand<ActiveContentItemViewModel>(async (acivm) =>
            {
                await ExecuteMovePreviousCommandAsync(acivm);
            },
            (acivm) =>
            {
                var canExecute = CanExecuteMovePreviousCommand(acivm);
                return canExecute;
            });
        }

        private async Task ExecuteMovePreviousCommandAsync(ActiveContentItemViewModel activeContentItemViewModel)
        {
            try
            {
                //await MovePreviousName();

                ExecuteBackByName(activeContentItemViewModel);
              
                await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message:$"{ex.Message}",title: "Failure:Move Previous");
            }
        }

        private bool CanExecuteMovePreviousCommand(ActiveContentItemViewModel activeContentItemViewModel)
        {
            if (activeContentItemViewModel is null)
            {
                return false;
            }

            var lastIndex = activeContentItemViewModel.Name.LastIndexOf(new string("."));
            return lastIndex >= -1;
        }

        private void ExecuteBackByName(ActiveContentItemViewModel activeContentItemViewModel)
        {
            RecursiveSubActiveContentItemViewModel(activeContentItemViewModel.Name);

             void RecursiveSubActiveContentItemViewModel(string name)
            {
                var lastIndex = name.LastIndexOf(".");
                if (lastIndex <= -1)
                {
                    return;
                }

                var parentName = activeContentItemViewModel.Name.Substring(0, lastIndex);
                //var contentItem = LeftPaneActiveContentViewModel.GetActiveContentItemByName(parentName);
                //if (contentItem is null)
                if (!LeftPaneActiveContentViewModel.ContainItemByName(parentName))
                {
                    RecursiveSubActiveContentItemViewModel(parentName);
                }
                else 
                {
                    LeftPaneActiveContentViewModel.SetSelectedItemByName(parentName);
                }
            }
        }

        private async Task MovePreviousName()
        {
            string previousContentItemName = default;

            //if (!RightContentActiveContentViewModel.HasActiveItem())
            //{
            //    RightContentActiveContentViewModel.SetActiveItemToLast();

            //    previousContentItemName = RightContentActiveContentViewModel.SelectedContentItem.Name;
            //}
            //else
            //{
            //    //previousContentItemName = RightContentActiveContentViewModel.GetActiveContentItemByName();

            //    //if (RightContentActiveContentViewModel.CanMovePrevious())
            //    //{
            //    //    RightContentActiveContentViewModel.ExecuteMovePrevious();
            //    //    previousContentItemName = RightContentActiveContentViewModel.SelectedContentItem.Name;
            //    //}
            //}

            //if (string.IsNullOrEmpty(name))
            //{
            //    if (LeftPaneActiveContentViewModel.CanMove)
            //    {
            //        LeftPaneActiveContentViewModel.ExecuteMovePrevious();
            //        var currentActiveContentItem = LeftPaneActiveContentViewModel.ActiveContentItems[LeftPaneActiveContentViewModel.SelectedIndex];
            //    }
            //    return;
            //}

            Debug.Assert(SelectedLeftPaneActiveContentItem== LeftPaneActiveContentViewModel.SelectedContentItem);

           await RecursiveSubActiveContent(LeftPaneActiveContentViewModel.SelectedContentItem);

            async Task RecursiveSubActiveContent(ActiveContentItemViewModel currentActiveContentItem)
            {
                var currentMenuSideBarView = currentActiveContentItem.ViewElement as HamburgerMenuSideBarView;
                var currentMenuSideBarViewModel = currentMenuSideBarView.DataContext as HamburgerMenuSideBarViewModel;

                var currentMenuSideBarItem = currentMenuSideBarViewModel.AllLeafHamburgerMenuSideBarItems.FirstOrDefault(msbi => msbi.Name == previousContentItemName);
                if (currentMenuSideBarItem is not null)
                {
                    if (currentMenuSideBarItem != currentMenuSideBarViewModel.SelectedHamburgerMenuSideBarItem && !currentMenuSideBarViewModel.SelectedHamburgerMenuSideBarItem.HasSubMenu)
                    {
                        SelectedHamburgerMenuSideBarItem = currentMenuSideBarItem;
                        currentMenuSideBarViewModel.SelectedHamburgerMenuSideBarItem = currentMenuSideBarItem;
                    }
                }
                else
                {
                    if (LeftPaneActiveContentViewModel.CanMovePrevious())
                    {
                       // LeftPaneActiveContentViewModel.SelectedIndex >= 1
                        //var previousContentItem = LeftPaneActiveContentViewModel.ActiveContentItems[LeftPaneActiveContentViewModel.SelectedIndex - 1];
                        LeftPaneActiveContentViewModel.ExecuteMovePrevious();

                        await RecursiveSubActiveContent(LeftPaneActiveContentViewModel.SelectedContentItem);
                    }
                }
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

        #region Create TopHamburgerMenuSideBar ViewModel Method
        private async Task CreateTopHamburgerMenuSideBarViewModelAsync()
        {
            IsLoading = true;

            try
            {
                //HamburgerMenuSideBar = new();

                var rootMenuItem = await _menuService.GetMenuAsync("All");
                var subMenuItems = rootMenuItem.SubMenus;
                TopHamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateTopHamburgerMenuSideBarViewModelAsync(subMenuItems);
               //AddHamburgerMenuSideBarViewModelPropertyChanged(TopHamburgerMenuSideBar);

                //List<HamburgerMenuSideBarItemViewModel> allideBarItemViewLeafs = new();

                //foreach (var leafSideBarItem in HamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems)
                //{

                //    //var sublLeafMenuItems = await leafSideBarItem.GetSubMenuAsync();

                //    //if (sublLeafMenuItems is not null && sublLeafMenuItems.Any())
                //    //{
                //    //    foreach (var smi in sublLeafMenuItems)
                //    //    {
                //    //        var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, leafSideBarItem, (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); }, true);
                //    //        var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                //    //        allideBarItemViewLeafs.AddRange(allTopItemLeafs);
                //    //    }

                //    //    var subHamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateHamburgerMenuSideBarViewModelAsync(menuItems: sublLeafMenuItems, parent: leafSideBarItem, keepParent: true);
                //    //}
                //}

                //var hamburgerMenuBarItemViewModels = await CreateHamburgerMenuBarItemViewModelsAsync();
                //HamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(hamburgerMenuBarItemViewModels);

                //HamburgerMenuSideBar.WorkspaceViewEventName = _workspaceViewEventName;
                //HamburgerMenuSideBar.SetWorkspaceViewEventName();

                //  await HamburgerMenuSideBar.CreateHamburgerMenuBarItemViewModelsAsync();
                TopHamburgerMenuSideBar.IsPaneOpen = IsPaneOpen;
                RaisePropertyChanged(nameof(HamburgerMenuSideBar));
            }
            catch (Exception ex)
            {
                await _dialogViewService.AlertAsync(message: $"Unable to create top hamburger menu : \"{ex.Message}\"", title: "Error: Create Top HamburgerMenuSideBar");
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

        #region Get All SubMenuSideBar ViewModels Method
        private async Task<List<(string Path, HamburgerMenuSideBarViewModel MenuSideBar)>> GetAllSubMenuSideBarViewModelsAsync(HamburgerMenuSideBarViewModel menuSideBar)
        {
            List<(string Path, HamburgerMenuSideBarViewModel MenuSideBar)> allMenuSideBars = new();
            NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();

            await RecursiveSubMenuItemViewModel(menuSideBar);

            async Task RecursiveSubMenuItemViewModel(HamburgerMenuSideBarViewModel currentMenuSideBar)
            {
                foreach (var leafSideBarItem in currentMenuSideBar.AllLeafHamburgerMenuSideBarItems)
                {
                    var sublLeafMenuItems = await leafSideBarItem.GetSubMenuAsync();

                    if (sublLeafMenuItems is not null && sublLeafMenuItems.Any())
                    {
                        List<HamburgerMenuSideBarItemViewModel> allBarItemLeafs = new();

                        string topItemName = default;
                        foreach (var smi in sublLeafMenuItems)
                        {
                            var topItem = await nodeResolver.GetTopItemByMenuItemAsync(menuItem: smi, parent: leafSideBarItem, childResolver: (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); }, isKeepParent: true);
                            topItemName = topItem.Path;
                        }

                        foreach (var topItem in leafSideBarItem.Children)
                        {
                            //string path = topItem.Path;
                            var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem as HamburgerMenuSideBarItemViewModel);
                            allBarItemLeafs.AddRange(allTopItemLeafs);
                        }

                        var subHamburgerMenuSideBar = new HamburgerMenuSideBarViewModel
                        {
                            AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allBarItemLeafs)
                        };

                        allMenuSideBars.Add((topItemName, subHamburgerMenuSideBar));

                        await RecursiveSubMenuItemViewModel(subHamburgerMenuSideBar);
                    }
                }
            }

            return allMenuSideBars;
        }
        #endregion

        #region Create HamburgerMenuItemBar ViewModel Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> CreateHamburgerMenuBarItemViewModelsAsync()
        {
            NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();

            List<HamburgerMenuSideBarItemViewModel> allLeafs = new();

            var rootMenuItem = await _menuService.GetMenuAsync("All");

            var subMenuItems = rootMenuItem.SubMenus;
            foreach (var smi in subMenuItems)
            {
                //var allMenuItemLeafs = await nodeResolver.GetMenuItemLeafsAsync(smi, (m) => { return new HamburgerMenuSideBarItemViewModel(m, null);});
                //allLeafs.AddRange(allMenuItemLeafs);
                //var allLeafsOfMenuItems = await GetLeafsOfMenuItemAsync(smi);
                // allLeafs.AddRange(allLeafsOfMenuItems);

                //var allTopMenuItemsLeafs = await nodeResolver.GetTopMenuItemLeafsAsync(smi, new HamburgerMenuSideBarItemViewModel(),(m,p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });
                //allLeafs.AddRange(allTopMenuItemsLeafs);
                //var allLeafsOfTopMenuItems = await GetLeafsOfTopMenuItemAsync(smi);
                //allLeafs.AddRange(allLeafsOfTopMenuItems);

                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(smi, new HamburgerMenuSideBarItemViewModel(), (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });
                var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                allLeafs.AddRange(allTopItemLeafs);
                //var topHeaderItem = await GetTopHeaderByMenuItemAsync(smi);
                //var allLeafsOfTopHeaderItems = await GetLeafsOfTopHeaderItemAsync(topHeaderItem);
                //allLeafs.AddRange(allLeafsOfTopHeaderItems);
            }

            var allDistinctLeafs = allLeafs.DistinctBy(item => (item.Name, item.Title)).ToList();
            allLeafs = new(allDistinctLeafs);

            return allLeafs;
        }
        #endregion

        #region Get Leafs Of MenuItem Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> GetLeafsOfMenuItemAsync(Infrastructure.MenuItem menuItem)
        {
            List<Infrastructure.MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();

            await RecursiveSubMenuItem(menuItem);

            async Task RecursiveSubMenuItem(Infrastructure.MenuItem currentMenuItem)
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

            bool HasSubMenu(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafsOfMenuItem;
        }
        #endregion

        #region Get Leafs Of Top MenuItem Method
        private async Task<IEnumerable<HamburgerMenuSideBarItemViewModel>> GetLeafsOfTopMenuItemAsync(Infrastructure.MenuItem menuItem)
        {
            List<Infrastructure.MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();
            HamburgerMenuSideBarItemViewModel virtualParent = new();

            await RecursiveSubMenuItem(menuItem, virtualParent);

            async Task RecursiveSubMenuItem(Infrastructure.MenuItem currentMenuItem, HamburgerMenuSideBarItemViewModel paren)
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

            bool HasSubMenu(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

            return leafsOfMenuItem;
        }
        #endregion

        #region Get Top Header Method
        private async Task<HamburgerMenuSideBarItemViewModel> GetTopHeaderByMenuItemAsync(Infrastructure.MenuItem menuItem)
        {
            List<Infrastructure.MenuItem> travelMenuItems = new();
            List<HamburgerMenuSideBarItemViewModel> leafsOfMenuItem = new();
            HamburgerMenuSideBarItemViewModel virtualParent = new();

            await RecursiveSubMenuItem(menuItem, virtualParent);

            async Task RecursiveSubMenuItem(Infrastructure.MenuItem currentMenuItem, HamburgerMenuSideBarItemViewModel paren)
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

            bool HasSubMenu(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

            bool IsLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.SubMenus.Count <= 0;

            bool HasTitle(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.Title);

            bool IsNextNavigation(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNextNavigation;

            bool HasNavigationName(Infrastructure.MenuItem mi) => (mi is not null) && !string.IsNullOrEmpty(mi.NavigationName);

            bool IsNexOnNotLeaf(Infrastructure.MenuItem mi) => (mi is not null) && mi.IsNexOnNotLeaf;

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

        private bool AnyEqualsMenuItems(IEnumerable<Infrastructure.MenuItem> menuItems, Infrastructure.MenuItem menuItem)
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
