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

        var result = await activeContentManager.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters);
        if (!result.IsAdd)
        {
           // throw new ArgumentNullException(result.Nessage);
            //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //{
            //    dialogViewService.AlertAsync(message: $"{result.Nessage} \".", title: $"Error:Add View").Await();
            //});
            dialogViewService.AlertAsync(message: $"{result.Nessage} \".", title: $"Error:Add View").Await();
        }
    }
    #endregion
}
