using Aksl.ActiveContents;
using Aksl.ActiveContents.ViewModels;
using Prism;
using Prism.Common;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using System.Windows;
using Unity;

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
            var unityContainerExtension = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<UnityContainerExtension>();
            //var container = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IUnityContainer>();
            var regionNavigationService = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<IRegionNavigationService>();

            ContentInformation contentInformation = new()
            {
                Name = menuItem.Name,
                Title = menuItem.Title,
                ViewName = menuItem.ViewName
            };

            var viewName = viewType.Name;
            var registeredView = unityContainerExtension.Instance.Resolve<object>(viewName);
           // var registeredView = container.Resolve<object>(viewName);
           // var registeredView = (PrismApplication.Current as PrismApplicationBase).Container.Resolve<object>(viewName);

            if (registeredView is FrameworkElement frameworkElement)
            {
                MvvmHelpers.AutowireViewModel(registeredView);

                if (navigationParameters is null)
                {
                    navigationParameters = new NavigationParameters();
                }

                NavigationContext navigationContext = new(regionNavigationService, new Uri(viewName, UriKind.RelativeOrAbsolute), navigationParameters);

                Action<INavigationAware> action = (n) => n.OnNavigatedTo(navigationContext);
                MvvmHelpers.ViewAndViewModelAction<INavigationAware>(registeredView, action);

                contentInformation.ViewName = null;
                contentInformation.ViewElement = frameworkElement;
            }

            return contentInformation;
        }

        return null;
    }
    #endregion

    #region Add View To Content Method
    //Task<(bool IsAdd, string Message)>
    public async Task AddViewToContentAsync(Infrastructure.MenuItem menuItem, ActiveContentViewModel activeContentViewModel, NavigationParameters navigationParameters = null)
    {
        //try
        //{
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
                // return (IsAdd: false, Message: $"{viewTypeAssemblyQualifiedName} is not find");
                //throw new ArgumentException($"Unable to create ContentInformation:{contentInformation.Name}");
                return;
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

            //return (true, null);
        }
        else
        {
            // exceptionHandler?.Invoke($"Missing Type : {viewTypeAssemblyQualifiedName}");
            throw new ArgumentException($"Error:Missing Type {viewTypeAssemblyQualifiedName}");

            //return (IsAdd: false, Message: $"{viewTypeAssemblyQualifiedName} is not find");
            //throw new Exception($"{viewTypeAssemblyQualifiedName} is not find");
        }
        //}
        //catch (KeyNotFoundException knfex)
        //{
        //    // return (IsAdd: false, Message: $"{ex.Message} is not find");
        //    throw new ArgumentException(knfex.Message);
        //}
        //catch (Exception ex)
        //{
        //   // return (IsAdd: false, Message: $"{ex.Message} is not find");
        //    throw new ArgumentException(ex.Message);
        //}
    }
    #endregion
}