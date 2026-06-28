using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public static class ServiceExtensions
{
    public static LoginHandler GetLoginHandler()
    {
        var loginHandler = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<LoginHandler>();

        return loginHandler;
    }

    public static IOptions<WebApiAddressSettings> GetWebApiAddressSettings()
    {
        var webApiAddressSettings = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<IOptions<WebApiAddressSettings>>();

        return webApiAddressSettings;
    }

    public static IDistributedCache GetMemoryDistributedCache()
    {
        var distributedCache = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<IDistributedCache>();

        return distributedCache;
    }

    public static WebApiProvider GetWebApiProvider()
    {
        var webApiProvider = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<WebApiProvider>();

        return webApiProvider;
    }

    public static JwtTokenProvider GetJwtTokenProvider()
    {
        var jwtTokenProvider = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<JwtTokenProvider>();

        return jwtTokenProvider;
    }

    public static Task<HttpClient> SetBaseAddressAsync(this HttpClient httpClient, string baseAddress)
    {
        httpClient.BaseAddress = new Uri(baseAddress);

        return Task.FromResult(httpClient);
    }

    public static Dictionary<string, string> SetBearer(this HttpClient httpClient, string accessToken)
    {
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Authorization", string.Format("Bearer {0}", accessToken));

        return header;
    }
}

