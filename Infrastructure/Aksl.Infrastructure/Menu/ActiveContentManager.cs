using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;

using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using Unity;

using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;

namespace Aksl.Infrastructure;

public class ActiveContentManager
{
    #region Create ContentInformation Method
    public async Task<ContentInformation> CreateContentInformationAsync(Infrastructure.MenuItem menuItem, NavigationParameters navigationParameters = null)
    {
        string viewTypeAssemblyQualifiedName = menuItem.ViewName;
        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
        if (viewType is not null)
        {
            var container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            var regionNavigationService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionNavigationService>();

            ContentInformation contentInformation = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                ViewName = menuItem.ViewName
            };

            var viewName = viewType.Name;

            var currentView = container.Resolve<object>(viewName);
            if (currentView is FrameworkElement frameworkElement)
            {
                MvvmHelpers.AutowireViewModel(currentView);

                // NavigationParameters navigationParameters = new() { { "CurrentMenuItem", menuItem } };
                if (navigationParameters is null)
                {
                    navigationParameters = new NavigationParameters();
                }

                var navigationContext = new NavigationContext(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute), navigationParameters);

                Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
                MvvmHelpers.ViewAndViewModelAction<INavigationAware>(currentView, action);

                contentInformation.ViewName = null;
                contentInformation.ViewElement = frameworkElement;
            }

            return contentInformation;
        }

        return null;
    }
    #endregion

    #region Add View To Content Method
    public async Task<string> AddViewToContentAsync(Infrastructure.MenuItem menuItem, ActiveContentViewModel activeContentViewModel, NavigationParameters navigationParameters = null)
    {
        string viewTypeAssemblyQualifiedName = menuItem.ViewName;
        Type viewType = Type.GetType(viewTypeAssemblyQualifiedName);
        if (viewType is not null)
        {
            var viewName = viewType.Name;

            ActiveContents.ContentInformation contentInformation = await CreateContentInformationAsync(menuItem, navigationParameters);
            if (contentInformation is null)
            {
                // exceptionHandler?.Invoke($"Missing Type : {viewTypeAssemblyQualifiedName}");
                // throw new ArgumentException($"Error:Missing Type {viewTypeAssemblyQualifiedName}");
                return ($"{viewTypeAssemblyQualifiedName} is not find");
            }

            var currentView = activeContentViewModel.GetStoreViewElementByName(menuItem.Name);
            if (currentView is not null)
            {
                if (menuItem.IsCacheable)
                {
                    activeContentViewModel.SetContentItem(contentInformation);
                }
                else
                {
                    activeContentViewModel.RetsetContentItem(contentInformation);
                }
            }
            else
            {
                activeContentViewModel.Add(contentInformation);
            }

            return null;
        }
        else
        {
            // exceptionHandler?.Invoke($"Missing Type : {viewTypeAssemblyQualifiedName}");
            //throw new ArgumentException($"Error:Missing Type {viewTypeAssemblyQualifiedName}");

            return ($"{viewTypeAssemblyQualifiedName} is not find");
        }
    }
    #endregion
}