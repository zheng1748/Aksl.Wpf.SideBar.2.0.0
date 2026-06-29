using Aksl.Dialogs.Services;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Unity;
using Unity;

namespace Aksl.Infrastructure;

public static class PrismUnityExtensions
{
    public static IRegionManager GetRegionManager()
    {
        return PrismIocExtensions.GetContainer().Resolve<IRegionManager>();
    }

    public static IEventAggregator GetEventAggregator()
    {
        return PrismIocExtensions.GetContainer().Resolve<IEventAggregator>();
    }

    public static IDialogViewService GetDialogViewService()
    {
        return PrismIocExtensions.GetContainer().Resolve<IDialogViewService>();
    }

    public static IMenuService GetMenuService()
    {
        return PrismIocExtensions.GetContainer().Resolve<IMenuService>();
    }
}

