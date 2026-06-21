using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;

using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Prism.Unity;

using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Unity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Aksl.Infrastructure;

public static class HttpClientExtensions
{
    public static LoginResolver GetLoginResolver()
    {
        var loginResolver = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<LoginResolver>();

        return loginResolver;
    }

    public static Task<WebApiProvider> GetWebApiProviderAsync()
    {
        var webApiProvider = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<WebApiProvider>();

        return Task.FromResult(webApiProvider);
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

    public static Task<JwtTokenProvider> GetJwtTokenProviderAsync()
    {
        var jwtTokenProvider = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                             ?.GetRequiredService<JwtTokenProvider>();

        return Task.FromResult(jwtTokenProvider);
    }

    public static Task<IHttpClientFactory> GetHttpClientFactoryAsync(string name)
    {
        var httpClientFactory = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                                ?.GetRequiredService<IHttpClientFactory>();

        return Task.FromResult(httpClientFactory);
    }

    public static async Task<HttpClient> GetWebApiClientAsync()
    {
        var httpClient = await CreateClientAsync("WebApi");

        return httpClient;

    }

    public static Task<HttpClient> CreateClientAsync()
    {
        var httpClient = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                          ?.GetRequiredService<IHttpClientFactory>()
                                                          ?.CreateClient();

        return Task.FromResult(httpClient);

    }
    public static Task<HttpClient> CreateClientAsync(string name)
    {
        var httpClient = PrismIocExtensions.GetContainer().Resolve<IServiceProvider>()
                                                          ?.GetRequiredService<IHttpClientFactory>()
                                                          ?.CreateClient(name);

        return Task.FromResult(httpClient);
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
    //public static Task<HttpClient> SetBaseAddressAsync(this HttpClient httpClient, string baseAddress)
    //{
    //    httpClient.DefaultRequestHeaders = new Uri(baseAddress);

    //    return Task.FromResult(httpClient);
    //}
}

