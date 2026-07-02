
using Prism.Ioc;
using Unity;

namespace Aksl.Infrastructure;

public static class ShellActiveContentExtensions
{
    public static void IfAccessTokenIsExpired()
    {
        if (ServiceExtensions.GetWebApiProvider().IsAccessTokenExpired)
        {
            RetsetActiveContentToLoginView();
        }
    }

    public static void RetsetActiveContentToLoginView()
    {
        var shellContentActiveContentViewModel = PrismUnityContainerExtensions.GetContainer().
                                                Resolve<ActiveContents.ViewModels.RandomActiveContentViewModel>(name: ActiveContentNames.ShellContent);

        shellContentActiveContentViewModel.RetsetContentItemByName("LoginView");
    }
}

