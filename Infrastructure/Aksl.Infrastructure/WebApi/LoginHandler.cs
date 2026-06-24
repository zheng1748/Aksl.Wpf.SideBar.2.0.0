using Aksl.Dialogs.Services;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public class LoginHandler
{
    #region Members
    private string _url;
    private WebApiProvider _webApiProvider;
    private JwtTokenProvider _jwtTokenProvider;
    private WebApiAddressSettings _webApiAddressSettings;
    #endregion

    #region Constructors
    //public LoginHandler(WebApiProvider webApiProvider, JwtTokenProvider lwtTokenProvider)
    //{
    //    _webApiProvider = webApiProvider;
    //    _jwtTokenProvider = lwtTokenProvider;
    //}

    public LoginHandler()
    {
        _webApiAddressSettings = ServiceExtensions.GetWebApiAddressSettings().Value;
    }
    #endregion

    #region Properties
    public WebApiProvider WebApiProvider { get; set; }

    public Action<string> ExecuteAccessTokenAction { get; set; }

    public Func<string, string, Task<LoginResponse>> ExecuteLoginActionAsync { get; set; }
    public Func<string,Task<ResetLockoutResponse>> ExecuteResetLockoutAsync { get; set; }
    public Func<Dictionary<string, string>,Task<GenerateEmailTokenResponse>> ExecuteGetEmailConfirmationTokenActionAsync { get; set; }
    #endregion

    #region Login Method
    public async Task<LoginResponse> LoginAsync(string userName, string password)
    {
        var loginResponse = await WebApiProvider.PostAsync<LoginResponse, LoginRequest>(_webApiAddressSettings.LoginUrl, new LoginRequest() { UserName = userName, Password = password });

        if (loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            ExecuteAccessTokenAction(loginResponse.AccessToken);
        }

        return loginResponse;
    }
    #endregion

    public async Task<ResetLockoutResponse> ResetLockoutAsync(string userName)
    {
        var resetLockoutResponse = await WebApiProvider.PostAsync<ResetLockoutResponse, ResetLockoutRequest>(_webApiAddressSettings.ResetLockoutUrl, new ResetLockoutRequest() { UserName = userName });

        return resetLockoutResponse;
    }

    public async Task<GenerateEmailTokenResponse> GetEmailConfirmationTokenAsync(Dictionary<string, string> parameters)
    {
        var getEmailConfirmationTokenUrl = HttpQueryParameter.Query(_webApiAddressSettings.GetEmailConfirmationTokenUrl, parameters).ToString();

        var generateEmailTokenResponse = await WebApiProvider.GetAsync<GenerateEmailTokenResponse>(getEmailConfirmationTokenUrl);

        return generateEmailTokenResponse;
    }
}

public static class LoginHandlerExtensions
{
    public static void AddLoginHandler(this IServiceCollection services)
    {
        services.AddHttpClient("WebApi");

        services.AddSingleton<WebApiProvider>((sp) =>
        {
            var logFactory = sp.GetRequiredService<ILoggerFactory>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

            WebApiProvider webApiProvider = new(httpClientFactory, logFactory.CreateLogger<WebApiProvider>());
            return webApiProvider;
        });

        services.AddTransient<LoginHandler>((sp) =>
        {
            var webApiProvider = sp.GetRequiredService<WebApiProvider>();

            LoginHandler loginHandler = new()
            {
                WebApiProvider = webApiProvider,
                ExecuteAccessTokenAction = (t) => webApiProvider.SetBearer(t),
            };

            loginHandler.ExecuteLoginActionAsync = loginHandler.LoginAsync;
            loginHandler.ExecuteResetLockoutAsync = loginHandler.ResetLockoutAsync;
            loginHandler.ExecuteGetEmailConfirmationTokenActionAsync = loginHandler.GetEmailConfirmationTokenAsync;
            return loginHandler;
        });
    }            
}

