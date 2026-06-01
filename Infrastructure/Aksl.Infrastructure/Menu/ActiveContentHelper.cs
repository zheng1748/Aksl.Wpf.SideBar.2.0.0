using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Aksl.Dialogs.Services;
using Aksl.Infrastructure;
using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using Unity;

namespace Aksl.Infrastructure;

public static class ActiveContentHelper
{
    #region Add View To Content Method
    public static async Task AddViewToContentAsync(Infrastructure.MenuItem menuItem,string activeContentNames)
    {
        var dialogViewService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IDialogViewService>();
        var contentActiveContentViewModel = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<ActiveContentViewModel>(name: activeContentNames);

        ActiveContentManager activeContentManager = new();
        //Action<string> exceptionHandler = (message) =>
        //{
        //    dialogViewService.AlertAsync(message: $"\"{message}\".", title: $"Error:Add View");
        //};
        NavigationParameters navigationParameters = new() { { "CurrentMenuItem", menuItem } };
        // activeContentManager.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters, exceptionHandler).Await();
        var message = await activeContentManager.AddViewToContentAsync(menuItem, contentActiveContentViewModel, navigationParameters);
        if (!string.IsNullOrEmpty(message))
        {
            await dialogViewService.AlertAsync(message: $"{message} \".", title: $"Error:Missing ViewType");
        }
    }
    #endregion
}
