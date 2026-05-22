using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Aksl.Modules.HamburgerMenuSideBar;

public static class HamburgerMenuSideBarHelper
{
    #region Create HamburgerMenuSideBarViewModel Method
    public static async Task<HamburgerMenuSideBarViewModel> CreateHamburgerMenuSideBarViewModelAsync(IEnumerable<Infrastructure.MenuItem> menuItems)
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

    #region GetS ubMenu Method
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

    #region LoadView To RightContent Method
    public static async Task LoadViewToRightContentAsync(MenuItem currentMenuItem)
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
                }
                else
                {
                    rightContentActiveContent.RetsetContentItem(contentInformation);
                }
            }
            else
            {
                rightContentActiveContent.Add(contentInformation);
            }
        }
        else
        {
            await dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type");
        }
    }
    #endregion
}
