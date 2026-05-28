using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;

using Prism;
using Prism.Ioc;
using Prism.Unity;

using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Aksl.Modules.HamburgerMenuSideBar.ViewModels;
using Aksl.Modules.HamburgerMenuSideBar.Views;

namespace Aksl.Modules.HamburgerMenuSideBar;

public static class HamburgerMenuSideBarHelper
{
    #region Create Top HamburgerMenuSideBarViewModel Method
    public static async Task<HamburgerMenuSideBarViewModel> CreateTopHamburgerMenuSideBarViewModelAsync(IEnumerable<Infrastructure.MenuItem> menuItems)
    {
        NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();
        HamburgerMenuSideBarViewModel hamburgerMenuSideBar = new();

        if (menuItems is not null && menuItems.Any())
        {
            List<HamburgerMenuSideBarItemViewModel> allSideBarItemLeafs = new();

            foreach (var mi in menuItems)
            {
                HamburgerMenuSideBarItemViewModel virtualParent = new();
                Func<MenuItem, HamburgerMenuSideBarItemViewModel, HamburgerMenuSideBarItemViewModel> constructorResolver = ((m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });

                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(mi, virtualParent, constructorResolver, false);
                var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                allSideBarItemLeafs.AddRange(allTopItemLeafs);
            }

            hamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allSideBarItemLeafs);
        }

        return hamburgerMenuSideBar;
    }
    #endregion

    #region Get All SubMenuSideBar ViewModels Method
    public static async Task<List<(string Path, HamburgerMenuSideBarViewModel MenuSideBar)>> GetAllSubMenuSideBarViewModelsAsync(HamburgerMenuSideBarViewModel menuSideBar)
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
                        var topItem = await nodeResolver.GetTopItemByMenuItemAsync(menuItem: smi, parent: leafSideBarItem, constructorResolver: (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); }, isKeepParent: true);
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

    #region Create HamburgerMenuSideBarViewModel Method
    public static async Task<HamburgerMenuSideBarViewModel> CreateHamburgerMenuSideBarViewModelAsync(IEnumerable<Infrastructure.MenuItem> menuItems, Func<MenuItem, HamburgerMenuSideBarItemViewModel> constructorResolver)
    {
        HamburgerMenuSideBarViewModel hamburgerMenuSideBar = new();

        if (menuItems is not null && menuItems.Any())
        {
            List<HamburgerMenuSideBarItemViewModel> allLeafs = new();

            NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();

            foreach (var mi in menuItems)
            {
                //var allMenuItemLeafs = await nodeResolver.GetMenuItemLeafsAsync(smi, (m) => { return new HamburgerMenuSideBarItemViewModel(m, null);});
                //allLeafs.AddRange(allMenuItemLeafs);

                //var allTopMenuItemsLeafs = await nodeResolver.GetTopMenuItemLeafsAsync(smi, new HamburgerMenuSideBarItemViewModel(),(m,p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });
                //allLeafs.AddRange(allTopMenuItemsLeafs);

                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(mi, new HamburgerMenuSideBarItemViewModel(), (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); });
                var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem);
                allLeafs.AddRange(allTopItemLeafs);
            }

            hamburgerMenuSideBar.AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allLeafs);
        }

        return hamburgerMenuSideBar;
    }
    #endregion

    #region Get SubMenu Method
    public static async Task<IEnumerable<Infrastructure.MenuItem>> GetSubMenuAsync(Infrastructure.MenuItem menuItem)
    {
        var menuService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IMenuService>();

        IEnumerable<Infrastructure.MenuItem> subMenuItems = default;

        if (!string.IsNullOrEmpty(menuItem.NavigationName))
        {
            var parentMenuItem = await menuService.GetMenuAsync(menuItem.NavigationName);
            subMenuItems = parentMenuItem.SubMenus;
        }

        if (string.IsNullOrEmpty(menuItem.NavigationName) && HasSubMenu(menuItem) && IsExistsViewInSubMenu(menuItem))
        {
            subMenuItems = menuItem.SubMenus.Where(sm => !string.IsNullOrEmpty(sm.ViewName)).ToList();
        }

        bool HasSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any();

        bool IsExistsViewInSubMenu(MenuItem mi) => (mi is not null) && mi.SubMenus.Any(sm => !string.IsNullOrEmpty(sm.ViewName));

        return subMenuItems;
    }
    #endregion

    #region Add Views To LeftPane Method
    public static async Task AddViewsToLeftPaneAsync(HamburgerMenuSideBarItemViewModel topuSideBarItem)
    {
        var leftPaneActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.LeftPaneHamburgerMenuSideBar);
        var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
        NodeResolver<HamburgerMenuSideBarItemViewModel> nodeResolver = new();

        var sublLeafMenuItems = await topuSideBarItem.GetSubMenuAsync();

        if (sublLeafMenuItems is not null && sublLeafMenuItems.Any())
        {
            List<HamburgerMenuSideBarItemViewModel> allBarItemLeafs = new();

            string topItemName = default;
            foreach (var smi in sublLeafMenuItems)
            {
                var topItem = await nodeResolver.GetTopItemByMenuItemAsync(menuItem: smi, parent: topuSideBarItem, constructorResolver: (m, p) => { return new HamburgerMenuSideBarItemViewModel(m, p); }, isKeepParent: true);
                topItemName = topItem.Path;
            }

            foreach (var topItem in topuSideBarItem.Children)
            {
                var allTopItemLeafs = await nodeResolver.GetTopItemLeafsAsync(topItem as HamburgerMenuSideBarItemViewModel);
                allBarItemLeafs.AddRange(allTopItemLeafs);
            }

            var subHamburgerMenuSideBar = new HamburgerMenuSideBarViewModel
            {
                AllLeafHamburgerMenuSideBarItems = new ObservableCollection<HamburgerMenuSideBarItemViewModel>(allBarItemLeafs)
            };

            ContentInformation contentInfo = new()
            {
                Name = topItemName,
                Title = topItemName,
                ViewName = "Aksl.Modules.HamburgerMenuSideBar.Views.HamburgerMenuSideBarView,Aksl.Modules.HamburgerMenuSideBar",
                ViewElement = new HamburgerMenuSideBarView() { DataContext = subHamburgerMenuSideBar }
            };

            leftPaneActiveContentViewModel.Add(contentInfo);
        }
    }
    #endregion

    #region Add View To RightContent Method
    public static async Task AddViewToRightContentAsync(Infrastructure.MenuItem currentMenuItem)
    {
        var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
        var rightContentActiveContent = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: ActiveContentNames.RightContentHamburgerMenuSideBar);

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

            var currentView = rightContentActiveContent.GetStoreViewElementByName(currentMenuItem.Name);

            if (currentView is not null)
            {
                if (currentMenuItem.IsCacheable)
                {
                    rightContentActiveContent.SetContentItem(contentInformation);

                  //  rightContentActiveContent.SetMoveIndexOnSet();
                }
                else
                {
                    rightContentActiveContent.RetsetContentItem(contentInformation);
                }
            }
            else
            {
                rightContentActiveContent.Add(contentInformation);

              //  rightContentActiveContent.SetMoveIndexOnAdd();
            }
        }
        else
        {
            await dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
        }
    }
    #endregion
}
