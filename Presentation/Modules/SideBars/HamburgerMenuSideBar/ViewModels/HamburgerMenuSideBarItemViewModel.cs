using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Xml.Linq;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;

using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;

using Aksl.Modules.HamburgerMenuSideBar.Views;

namespace Aksl.Modules.HamburgerMenuSideBar.ViewModels;

public class HamburgerMenuSideBarItemViewModel : NodeViewModel
{
    #region Members
    protected readonly IEventAggregator _eventAggregator;
    private readonly IDialogViewService _dialogViewService;
    private readonly IMenuService _menuService;
    private readonly Aksl.Infrastructure.MenuItem _menuItem;
    #endregion

    #region Constructors
    public HamburgerMenuSideBarItemViewModel() : base()
    {
        _eventAggregator = PrismIocExtensions.GetContainer().Resolve<IEventAggregator>();
        _dialogViewService = PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();
        _menuService = PrismIocExtensions.GetContainer().Resolve<IMenuService>();

        _menuItem = null;
    }

    public HamburgerMenuSideBarItemViewModel(Aksl.Infrastructure.MenuItem menuItem, HamburgerMenuSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
    {
        _eventAggregator = PrismIocExtensions.GetContainer().Resolve<IEventAggregator>();
        _dialogViewService = PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();
        _menuService = PrismIocExtensions.GetContainer().Resolve<IMenuService>();

        _menuItem = menuItem;
    }
    #endregion

    #region Properties
    public Aksl.Infrastructure.MenuItem MenuItem => _menuItem;
  //  public bool HasViewName => !string.IsNullOrEmpty(_menuItem.ViewName);
    public string WorkspaceViewEventName { get; set; }
    //public int Level => _menuItem.Level;
    public string NavigationName => _menuItem.NavigationName;
    public bool IsSelectedOnInitialize => _menuItem.IsSelectedOnInitialize;

    //public PackIconKind IconKind =>
    //                    _menuItem.GetIconKind();
    public PackIconKind IconKind =>
                      _menuItem.IconKind.ToPackIconKind();

    public bool HasSubMenu =>
                       _menuItem.HasNextSubMenu();

    public bool HasViewName =>
                       _menuItem.HasViewName();

    public bool IsSetLeftPaneActiveContentItem =>
                            IsSelected && _menuItem.HasNextSubMenu() && !_menuItem.IsNexApplication;

    public bool IsAddViewToRightContent => 
                           IsSelected && IsLeaf && !_menuItem.HasNextSubMenu() && HasViewName && !_menuItem.IsNexApplication;

    public bool IsNavigationToRightContent =>
                           IsSelected && IsLeaf && _menuItem.HasNextSubMenu() && HasViewName && _menuItem.IsNexApplication;

    private bool _isSelected = false;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (SetProperty<bool>(ref _isSelected, value))
            {
                if (IsAddViewToRightContent)
                {
                    AddViewToRightContent();
                }

                if (IsNavigationToRightContent)
                {
                    NavigationToRightContent();
                }

                if (IsSetLeftPaneActiveContentItem)
                {
                    SetLeftPaneActiveContentItem();
                }

                //bool IsAddViewToRightContent()
                //{
                //    //return !HasSubMenu && IsLeaf && IsSelected && !string.IsNullOrEmpty(_menuItem.ViewName);
                //    return IsSelected && IsLeaf && !HasSubMenu && HasViewName;
                //}

                //bool IsSetLeftPaneActiveContentItem()
                //{
                //    return IsSelected && HasSubMenu;
                //}
            }
        }
    }

    private bool _isPaneOpen = false;
    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => SetProperty<bool>(ref _isPaneOpen, value);
    }

    protected bool _isEnabled = true;
    public bool IsEnabled
    {
        get => _isEnabled;

        set => SetProperty<bool>(ref _isEnabled, value);
    }
    #endregion

    #region Add View To RightContent Method
    public void AddViewToRightContent()
    {
        ActiveContentManagerExtensions.AddViewToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuSideBar).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
            {
                await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To RightContent");
            });
        });
    }
    #endregion

    #region Navigation To RightContent Method
    public void NavigationToRightContent()
    {
        ActiveContentManagerExtensions.NavigationToRandomContentAsync(_menuItem, ActiveContentNames.RightContentHamburgerMenuSideBar, new() { { "CurrentMenuItem", _menuItem } }).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
            {
                await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To RightContent");
            });
        });
    }
    #endregion

    #region Set LeftPane Active ContentItem Method
    public void SetLeftPaneActiveContentItem()
    {
        var leftPaneActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<SequenceActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);

        if (leftPaneActiveContentViewModel.ContainItemByName(this.Path))
        {
            leftPaneActiveContentViewModel.SetContentItemByName(this.Path);
        }
        else
        {
            HamburgerMenuSideBarHelper.AddViewsToLeftPaneAsync(this).Await(completedCallback: null, configureAwait: true, errorCallback: (ex) =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
                {
                    await _dialogViewService.AlertAsync(message: $"{ex.Message} \".", title: $"Error:Add View To LeftContent");
                });
            });
        }
    }
    #endregion

    #region Get SubMenu Method
    private bool HasSubMenuInternal()
    {
        //var subMenuItems = HamburgerMenuSideBarHelper.GetSubMenuAsync(_menuItem).GetAwaiter().GetResult();

        //return subMenuItems is not null && subMenuItems.Any();

        var hasSubMenu = (!string.IsNullOrEmpty(_menuItem.NavigationName)) || (string.IsNullOrEmpty(_menuItem.NavigationName) && HasSubMenu(_menuItem) && IsExistsViewInSubMenu(_menuItem));

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

        return hasSubMenu;
    }

    public async Task<IEnumerable<Infrastructure.MenuItem>> GetSubMenuAsync()
    {
        IEnumerable<Infrastructure.MenuItem> subMenuItems = new List<Infrastructure.MenuItem>();

        if (!string.IsNullOrEmpty(_menuItem.NavigationName))
        {
            var parentMenuItem = await _menuService.GetMenuAsync(_menuItem.NavigationName);
            subMenuItems = parentMenuItem.SubMenus;
        }

        if (string.IsNullOrEmpty(_menuItem.NavigationName) && HasSubMenu(_menuItem) && IsExistsViewInSubMenu(_menuItem))
        {
            subMenuItems = _menuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
        }

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

        return subMenuItems;
    }
    #endregion

    #region Add View To LeftPane Method
    public HamburgerMenuSideBarViewModel HamburgerMenuSideBar { get; set; }

    private HamburgerMenuSideBarItemViewModel _selectedHamburgerMenuSideBarItem;
    public HamburgerMenuSideBarItemViewModel SelectedHamburgerMenuSideBarItem
    {
        get => _selectedHamburgerMenuSideBarItem;
        set
        {
            if (SetProperty(ref _selectedHamburgerMenuSideBarItem, value))
            {
                if (_selectedHamburgerMenuSideBarItem is not null)
                {
                }
            }
        }
    }

    private async Task AddViewToLeftPaneAsync()
    {
        var leftPaneActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
        string name = $" {this.Path}.{nameof(HamburgerMenuSideBarView)}";

        ContentInformation contentInformation = new() { Name = name, Title = name };

        var currentView = leftPaneActiveContentViewModel.GetStoreViewElementByName(name);
        if (currentView is not null)
        {
            leftPaneActiveContentViewModel.SetContentItem(contentInformation);
        }
        else
        {
            await CreateHamburgerMenuSideBarViewModelAsync();
            //contentInformation.ViewElement = new HamburgerMenuSideBarView() { DataContext = HamburgerMenuSideBar };
            //leftPaneActiveContentViewModel.Add(contentInformation);
        }

        async Task CreateHamburgerMenuSideBarViewModelAsync()
        {
            //IEnumerable<Infrastructure.MenuItem> subMenuItems = await GetSubMenuAsync();
            IEnumerable<Infrastructure.MenuItem> subMenuItems = await HamburgerMenuSideBarHelper.GetSubMenuAsync(_menuItem);
            if (subMenuItems is not null && subMenuItems.Any())
            {
                HamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateTopHamburgerMenuSideBarViewModelAsync(subMenuItems);
                //List<HamburgerMenuSideBarItemViewModel> allLeafs = new();
                //HamburgerMenuSideBarViewModel hamburgerMenuSideBar = new();

                //foreach (var smi in subMenuItems)
                //{
                //    var allLeafsOfTopMenuItems = await GetLeafsOfTopMenuItemAsync(smi);
                //    allLeafs.AddRange(allLeafsOfTopMenuItems);
                //}

                //hamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allLeafs);

                // AddPropertyChanged();
                void AddPropertyChanged()
                {
                    HamburgerMenuSideBar.PropertyChanged += (sender, e) =>
                    {
                        if (sender is HamburgerMenuSideBarViewModel hmbvm)
                        {
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

                contentInformation.ViewElement = new HamburgerMenuSideBarView() { DataContext = HamburgerMenuSideBar };
                leftPaneActiveContentViewModel.Add(contentInformation);

                //leftPaneActiveContentViewModel.Add(new()
                //{
                //    Name = $" {this.Name}.{nameof(HamburgerMenuSideBarView)}",
                //    Title = $" {this.Name}.{nameof(HamburgerMenuSideBarView)}",
                //    ViewName = "",
                //    ViewElement = new HamburgerMenuSideBarView() { DataContext = HamburgerMenuSideBar }
                //});
            }
        }
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
}

