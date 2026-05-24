using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Infrastructure.Events;
using Aksl.Modules.HamburgerMenuSideBar.Views;
using Aksl.Toolkit.Controls;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;
using Unity;

namespace Aksl.Modules.HamburgerMenuSideBar.ViewModels
{
    public class NodeViewModel : BindableBase
    {
        #region Members;
        #endregion

        #region Constructors
        public NodeViewModel() 
        {
            Parent = null;
            Name = "VirtualNode";

            Children = new();
            Path = "Root";
        }

        public NodeViewModel(string name, string title, NodeViewModel parent)
        {
            Name = name;
            Title = title;

            Parent = parent;
            Parent?.Children.Add(this);
            Children = new();

            Path= Parent is not null ? $"{Parent.Path}.{Name}" : Name;
        }

        //public NodeViewModel(string name, string title ) : this(name,title,null)
        //{
        //}

        //public NodeViewModel(string name, string title, NodeViewModel parent)
        //{
        //    Name = name;
        //    Title = title;

        //    _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();

        //    _children = new((from child in _menuItem.SubMenus
        //                     select new NodeViewModel( child.Name, child.Title, this)).ToList<NodeViewModel>());

        //}
        #endregion

        #region Properties
        public string Name { get; set; }
        public string Path { get; set; }
        public string Title { get; set; }
        public int Level { get; set; }
        public virtual NodeViewModel Parent { get; set; }
        public virtual ObservableCollection<NodeViewModel> Children { get; set; }
        public bool HasTitle => !string.IsNullOrEmpty(Title);
        public bool HasChildren => (Children is not null) && Children.Any();
        public bool IsLeaf => (Children is not null) && Children.Count <= 0;
        public bool IsTopLevelItem => (Parent is null) && IsLeaf;
        public bool IsTopLevelHeader => (Parent is null) && !IsLeaf;
        public bool IsSubmenuItem => (Parent is not null) && IsLeaf;
        public bool IsSubmenuHeader => (Parent is not null) && !IsLeaf;
        #endregion
    }

    public class HamburgerMenuSideBarItemViewModel : NodeViewModel
    {
        #region Members
        protected readonly IEventAggregator _eventAggregator;
        private readonly IMenuService _menuService;
        //protected readonly HamburgerMenuSideBarItemViewModel _parent;
        //protected ObservableCollection<HamburgerMenuSideBarItemViewModel> _children;
        private readonly MenuItem _menuItem;
        #endregion

        #region Constructors
        public HamburgerMenuSideBarItemViewModel():base()
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _menuService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IMenuService>();
            _menuItem = null;
            //Parent = null;

            //_children = new();
        }

        public HamburgerMenuSideBarItemViewModel(MenuItem menuItem, HamburgerMenuSideBarItemViewModel parent) : base(menuItem.Name, menuItem.Title, parent)
        {
            _eventAggregator = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IEventAggregator>();
            _menuService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IMenuService>();

            _menuItem = menuItem;

            //Parent = parent;
            //Parent?.Children.Add(this);

            //_children = new();
        }

        //public HamburgerMenuSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem) : this(eventAggregator, menuItem, null)
        //{
        //    RaisePropertyChanged(nameof(IsLeaf));
        //}

        //public HamburgerMenuSideBarItemViewModel(IEventAggregator eventAggregator, MenuItem menuItem, HamburgerMenuSideBarItemViewModel parent)
        //{
        //    _eventAggregator = eventAggregator;
        //    _menuItem = menuItem;
        //    _parent = parent;

        //    _children = new((from child in _menuItem.SubMenus
        //                     select new HamburgerMenuSideBarItemViewModel(eventAggregator, child, this)).ToList<HamburgerMenuSideBarItemViewModel>());

        //    RaisePropertyChanged(nameof(IsLeaf));
        //}
        #endregion

        #region Properties
        public MenuItem MenuItem => _menuItem;
        //public string IconPath => _menuItem.IconPath;
        //public string Name => _menuItem.Name;
        public bool HasViewName =>!string.IsNullOrEmpty( _menuItem.ViewName);
        public string WorkspaceViewEventName { get; set; }
        //public int Level => _menuItem.Level;
        public string NavigationName => _menuItem.NavigationName;
        public bool IsSelectedOnInitialize => _menuItem.IsSelectedOnInitialize;
        public bool HasSubMenu
        {
            get
            {
               return HasSubMenuInternal();
            }
        }
        //public HamburgerMenuSideBarItemViewModel Parent { get; set; }
        //public ObservableCollection<HamburgerMenuSideBarItemViewModel> Children => _children;
        //public bool HasTitle => !string.IsNullOrEmpty(_menuItem.Title);
        //public bool HasChildren => (_children is not null) && _children.Any();
        //public bool IsLeaf => (_children is not null) && _children.Count <= 0;
        //public bool IsTopLevelItem => (Parent is null) && IsLeaf;
        //public bool IsTopLevelHeader => (Parent is null) && !IsLeaf;
        //public bool IsSubmenuItem => (Parent is not null) && IsLeaf;
        //public bool IsSubmenuHeader => (Parent is not null) && !IsLeaf;

        private bool _isSelected = false;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value))
                {
                    if (!HasSubMenu && IsLeaf && _isSelected )
                    {
                        //var buildHWorkspaceViewEvent = _eventAggregator.GetEvent(WorkspaceViewEventName) as OnBuildWorkspaceViewEventbase;
                        //buildHWorkspaceViewEvent.Publish(new() { CurrentMenuItem = _menuItem });

                        //HamburgerMenuSideBarHelper.AddViewToRightContentAsync(_menuItem).Await();
                    }

                    if (HasSubMenu && _isSelected)
                    {
                        //AddViewToLeftPaneAsync().Await();
                    }
                }
            }
        }

        public PackIconKind IconKind
        {
            get
            {
                PackIconKind kind = PackIconKind.None;

                _ = Enum.TryParse(_menuItem.IconKind, out kind);

                return kind;
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

        #region Load View To LeftPane Method
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

        private bool HasSubMenuInternal()
        {
            //var subMenuItems = HamburgerMenuSideBarHelper.GetSubMenuAsync(_menuItem).GetAwaiter().GetResult();

            //return subMenuItems is not null && subMenuItems.Any();

           var hasSubMenu=(!string.IsNullOrEmpty(_menuItem.NavigationName)) || (string.IsNullOrEmpty(_menuItem.NavigationName) && HasSubMenu(_menuItem) && IsExistsViewInSubMenu(_menuItem));

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
                    HamburgerMenuSideBar = await HamburgerMenuSideBarHelper.CreateHamburgerMenuSideBarViewModelAsync(subMenuItems);
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
}
