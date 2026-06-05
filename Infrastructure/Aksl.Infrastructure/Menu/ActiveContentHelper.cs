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

public static class ActiveContentHelper
{
    #region Add View To Content Method
    public static async Task AddViewToContentAsync(Infrastructure.MenuItem menuItem, string activeContentNames, NavigationParameters navigationParameters = null)
    {
        var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
        var contentActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: activeContentNames);

        ActiveContentManager activeContentManager = new();

        if (navigationParameters is null)
        {
            navigationParameters = new() { { "CurrentMenuItem", menuItem } };
        }

        try
        {
            await activeContentManager.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters);
        }
        catch (Exception ex)
        {
            string msg = !string.IsNullOrEmpty(ex.InnerException?.Message) ? ex.InnerException.Message : ex.Message;

            dialogViewService.AlertAsync(message: $"{msg} \".", title: $"Add View:\"{menuItem.Title}\"").Await();
        }
        //var result = await activeContentManager.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters);
        //if (!result.IsAdd)
        //{
        //   // throw new ArgumentNullException(result.Nessage);
        //    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //    //{
        //    //    dialogViewService.AlertAsync(message: $"{result.Nessage} \".", title: $"Error:Add View").Await();
        //    //});
        //    dialogViewService.AlertAsync(message: $"{result.Message} \".", title: $"Error:Add View").Await();
        //}
    }
    #endregion
}
