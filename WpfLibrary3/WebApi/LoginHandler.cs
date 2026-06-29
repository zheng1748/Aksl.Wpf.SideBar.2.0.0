using Aksl.Dialogs.Services;
using Aksl.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
    private ILogger<LoginHandler> _logger;
    private WebApiAddressSettings _webApiAddressSettings;
    #endregion

    #region Constructors
    public LoginHandler(WebApiProvider webApiProvider, IOptions<WebApiAddressSettings> webApiAddressOption,ILogger<LoginHandler> logger)
    {
        WebApiProvider = webApiProvider;
       _webApiAddressSettings = webApiAddressOption.Value;
       _logger = logger;

        BuildActions();
    }

    //public LoginHandler()
    //{
    //    _webApiAddressSettings = ServiceExtensions.GetWebApiAddressSettings().Value;
    //    WebApiProvider = ServiceExtensions.GetWebApiProvider();

    //    BuildActions();
    //}
    #endregion

    #region Properties
    public WebApiProvider WebApiProvider { get; set; }
    public Action<string,string> BindAccessTokenAction { get; set; }
    public Func<string, string, Task<LoginResponse>> ExecuteLoginAction { get; set; }
    public Func<string, Task<LoginOutResponse>> ExecuteLoginOutAction { get; set; }
    public Func<string, string, Task<RefreshTokenResponse>> ExecuteRefreshTokenAction { get; set; }
    public Func<string,Task<ResetLockoutResponse>> ExecuteResetLockoutAction { get; set; }
    public Func<HttpQueryKeyValuePair[], Task<GenerateEmailTokenResponse>> ExecuteGetEmailConfirmationTokenAction { get; set; }
    #endregion

    #region Build Action Method
    public void BuildActions()
    {
        BindAccessTokenAction = (ak,rk) => WebApiProvider.SetBearer(ak, rk);
        ExecuteLoginAction = LoginAsync;
        ExecuteLoginOutAction = LoginOutAsync;
        ExecuteRefreshTokenAction = RefreshTokenAsync;
        ExecuteResetLockoutAction = ResetLockoutAsync;
        ExecuteGetEmailConfirmationTokenAction = GetEmailConfirmationTokenAsync;
    }
    #endregion

    #region Login Method
    public async Task<LoginResponse> LoginAsync(string userName, string password)
    {
        var loginResponse = await WebApiProvider.
                        PostAsync<LoginResponse, LoginRequest>(_webApiAddressSettings.LoginUrl, 
                                        new LoginRequest() { UserName = userName, Password = password ,RefreshToken=WebApiProvider.RefreshToken });

        if (loginResponse.Succeeded && !string.IsNullOrEmpty(loginResponse.AccessToken))
        {
            BindAccessTokenAction(loginResponse.AccessToken, loginResponse.RefreshToken);

            _logger.LogInformation($"Execute Login Method AccessToken :{loginResponse.AccessToken} RefreshToken :{loginResponse.RefreshToken} From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            _logger.LogInformation($"Execute Login Method Errors : {loginResponse.ToString()} From {_webApiAddressSettings.LoginUrl}");
        }

        return loginResponse;
    }
    #endregion

    #region LoginOut Method
    public async Task<LoginOutResponse> LoginOutAsync(string userName)
    {
        var loginOutResponse = await WebApiProvider.PostAsync<LoginOutResponse, LoginOutRequest>(_webApiAddressSettings.LoginOutUrl,
                                        new LoginOutRequest() { UserName = userName, AccessToken = WebApiProvider.AccessToken, RefreshToken = WebApiProvider.RefreshToken });

        if (loginOutResponse.Succeeded)
        {
            BindAccessTokenAction(null, null);

            _logger.LogInformation($"Execute LoginOut Method Succeeded From {_webApiAddressSettings.LoginUrl}");
        }
        else
        {
            BindAccessTokenAction(null, null);

            _logger.LogInformation($"Execute LoginOut Method Succeeded From {_webApiAddressSettings.LoginUrl}");
        }

        return loginOutResponse;
    }
    #endregion

    #region RefreshToken Method
    public async Task<RefreshTokenResponse> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var refreshTokenResponse = await WebApiProvider.
                 PostAsync<RefreshTokenResponse,RefreshTokenRequest>(_webApiAddressSettings.RefreshTokenUrl, new RefreshTokenRequest() { AccessToken = accessToken, RefreshToken = refreshToken });

        if (refreshTokenResponse.Succeeded && !string.IsNullOrEmpty(refreshTokenResponse.AccessToken))
        {
            BindAccessTokenAction(refreshTokenResponse.AccessToken, refreshTokenResponse.RefreshToken);

            _logger.LogInformation($"Execute RefreshToken Method AccessToken :{refreshTokenResponse.AccessToken} RefreshToken :{refreshTokenResponse.RefreshToken} From {_webApiAddressSettings.RefreshTokenUrl}");
        }
        else
        {
            _logger.LogInformation($"Execute RefreshToken Method Errors : {refreshTokenResponse.ToString()} From {_webApiAddressSettings.RefreshTokenUrl}");
        }

        return refreshTokenResponse;
    }
    #endregion

    #region ResetLockout Method
    public async Task<ResetLockoutResponse> ResetLockoutAsync(string userName)
    {
        var resetLockoutResponse = await WebApiProvider.PostAsync<ResetLockoutResponse, ResetLockoutRequest>(_webApiAddressSettings.ResetLockoutUrl, new ResetLockoutRequest() { UserName = userName });

        _logger.LogInformation("Execute ResetLock Method");

        return resetLockoutResponse;
    }
    #endregion

    #region GetEmailConfirmationToken Method
    //public async Task<GenerateEmailTokenResponse> GetEmailConfirmationTokenAsync(Dictionary<string, string> parameters)
    public async Task<GenerateEmailTokenResponse> GetEmailConfirmationTokenAsync(HttpQueryKeyValuePair[] parameters)
    {
        var getEmailConfirmationTokenUrl = HttpQueryParameter.Query(_webApiAddressSettings.GetEmailConfirmationTokenUrl, parameters).ToString();

        var generateEmailTokenResponse = await WebApiProvider.GetAsync<GenerateEmailTokenResponse>(getEmailConfirmationTokenUrl);

        _logger.LogInformation("Execute GetEmailConfirmationToken Method");

        return generateEmailTokenResponse;
    }
    #endregion
}

public static class LoginHandlerExtensions
{
    public static void AddLoginHandler(this IServiceCollection services)
    {
        services.AddHttpClient("WebApi");

        services.AddSingleton<WebApiProvider>();

        services.AddSingleton<LoginHandler>();

        //services.AddSingleton<WebApiProvider>((sp) =>
        //{
        //    var logFactory = sp.GetRequiredService<ILoggerFactory>();
        //    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

        //    WebApiProvider webApiProvider = new(httpClientFactory, logFactory.CreateLogger<WebApiProvider>());
        //    return webApiProvider;
        //});

        //services.AddTransient<LoginHandler>((sp) =>
        //{
        //    var webApiProvider = sp.GetRequiredService<WebApiProvider>();

        //    LoginHandler loginHandler = new()
        //    {
        //        WebApiProvider = webApiProvider
        //    };

        //    //LoginHandler loginHandler = new()
        //    //{
        //    //    WebApiProvider = webApiProvider,
        //    //    ExecuteAccessTokenAction = (t) => webApiProvider.SetBearer(t),
        //    //};

        //    //loginHandler.ExecuteLoginActionAsync = loginHandler.LoginAsync;
        //    //loginHandler.ExecuteResetLockoutAsync = loginHandler.ResetLockoutAsync;
        //    //loginHandler.ExecuteGetEmailConfirmationTokenActionAsync = loginHandler.GetEmailConfirmationTokenAsync;

        //    return loginHandler;
        //});
    }
}

