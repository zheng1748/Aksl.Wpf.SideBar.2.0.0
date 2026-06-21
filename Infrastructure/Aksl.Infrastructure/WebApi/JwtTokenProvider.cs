using Aksl.Infrastructure.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;

namespace Aksl.Infrastructure;

public class JwtTokenProvider
{
    #region Members
    private IHttpClientFactory _httpClientFactory;
    private ILogger<JwtTokenProvider> _logger;
    #endregion

    #region Constructors
    public JwtTokenProvider(IHttpClientFactory httpClientFactory , ILogger<JwtTokenProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentException("HttpClientFactory is not null");
        _logger = logger ?? NullLoggerFactory.Instance.CreateLogger<JwtTokenProvider>();
    }
    #endregion

    #region Properties
    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }
    #endregion

    public Dictionary<string, string> SetBearer()
    {
        Dictionary<string, string> header = new()
            {
              { "Authorization",string.Format("Bearer {0}", AccessToken )}
            };

        return header;
    }

    #region Post Method
    public async Task<bool> GetTokenAsync(string url, string userName, string password)
    {
        //try
        //{
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        LoginRequest loginRequest = new() { UserName = userName, Password = password };

        var loginRequestJosn = await JsonSerializerHelper.SerializeStringAsync<LoginRequest>(loginRequest);
        HttpContent content = new StringContent(loginRequestJosn);
      
        var response = await httpClient.PostAsJsonAsync<LoginRequest>(url, loginRequest);
        //var response = await httpClient.PostAsync(url, content);
        //  response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var loginResponse = await JsonSerializer.DeserializeAsync<LoginResponse>(stream);

            AccessToken = loginResponse.AccessToken;
            RefreshToken = loginResponse.RefreshToken;

            return true;
        }
        else
        {
            //return false;
            throw new HttpRequestException(response.StatusCode.ToString());
        }

       // _logger.LogInformation($"GenerateAccessToken:{AccessToken},GenerateRefreshToken:{RefreshToken}");
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError($"GenerateAccessToken:{ex.Message}");

        //    throw new Exception($"HttpPost:{url} Error", ex);
        //}
    }
    #endregion
}

