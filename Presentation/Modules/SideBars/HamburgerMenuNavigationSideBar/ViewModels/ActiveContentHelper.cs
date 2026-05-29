using Aksl.ActiveContentManager;
using Aksl.ActiveContentManager.ViewModels;
using Aksl.Dialogs.Services;
using Prism;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Media3D;

namespace Aksl.Modules.HamburgerMenuNavigationSideBar.ViewModels;

public static class ActiveContentHelper
{
    #region Add View To Content Method
    public static async Task AddViewToContentAsync(Infrastructure.MenuItem currentMenuItem,string activeContentName, IDialogViewService dialogViewService)
    {
        var rightContentActiveContent = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: activeContentName);

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
            await dialogViewService.AlertAsync(message: $"Unable to find \"{viewTypeAssemblyQualifiedName}\".", title: $"Error:Missing Type",width:650,height:300,okText:"确定");
        }
    }
    #endregion
}
