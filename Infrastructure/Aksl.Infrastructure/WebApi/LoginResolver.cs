using Aksl.Dialogs.Services;
using Aksl.Infrastructure.Models;
using Aksl.Toolkit.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using Unity;

namespace Aksl.Infrastructure;

public class LoginResolver
{
    #region Members
    private string _url;
    private WebApiProvider _webApiProvider;
    private JwtTokenProvider _lwtTokenProvider;
    #endregion

    #region Constructors
    public LoginResolver(WebApiProvider webApiProvider, JwtTokenProvider lwtTokenProvider)
    {
        _webApiProvider = webApiProvider;
        _lwtTokenProvider = lwtTokenProvider;
    }
    #endregion

    #region Properties
    public string LoginUrl { get; set; }
    #endregion

    #region Login Method
    public async Task LoginAsync(string userName, string password)
    {
        if (string.IsNullOrEmpty(LoginUrl))
        {
            throw new ArgumentException("login url is require");
        }

        _ = await _lwtTokenProvider.GetTokenAsync(LoginUrl, userName, password);

        if (!string.IsNullOrEmpty(_lwtTokenProvider.AccessToken))
        {
            _webApiProvider.HeaderProperties.SetString("Authorization", $"Bearer {_lwtTokenProvider.AccessToken}");
        }
    }
    #endregion
}

