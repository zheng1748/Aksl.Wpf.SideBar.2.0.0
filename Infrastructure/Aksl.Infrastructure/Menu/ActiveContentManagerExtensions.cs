using System;
using System.Threading.Tasks;

using Prism;
using Prism.Ioc;
using Prism.Regions;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;

namespace Aksl.Infrastructure;

public static class ActiveContentManagerExtensions
{
    #region Add View To Content Method
    public static async Task AddViewToContentAsync(Infrastructure.MenuItem menuItem, string activeContentNames, NavigationParameters navigationParameters = null)
    {
        var contentActiveContentViewModel = PrismIocExtensions.GetContainer().Resolve<ActiveContentViewModel>(name: activeContentNames);

        if (navigationParameters is null)
        {
            navigationParameters = new() { { "CurrentMenuItem", menuItem } };
        }

        try
        {
            await ActiveContentManager.Instance.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters);
        }
        catch (Exception ex)
        {
            string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

            throw new Exception(msg);
        }
    }
    #endregion
}
