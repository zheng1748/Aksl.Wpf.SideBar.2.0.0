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
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Resources;
using System.Xml;

namespace Aksl.Infrastructure;
//Wrapper
public class WebApiProvider
{
    #region Members
    private IHttpClientFactory _httpClientFactory;
    private ILogger<WebApiProvider> _logger;
    #endregion

    #region Constructors
    public WebApiProvider(IHttpClientFactory httpClientFactory, ILogger<WebApiProvider> logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentException("HttpClientFactory is not null"); 
        _logger = logger?? NullLoggerFactory.Instance.CreateLogger<WebApiProvider>();

        HeaderProperties=new();
    }
    #endregion

    #region Properties
    public HeaderProperties HeaderProperties { get; set; }
    #endregion

    #region Post Method
    public async Task<T> PostAsync<T, V>(V v, string requestUrl, HeaderProperties headerProperties = null, int timeoutSecond = 180,CancellationToken cancellationToken = default)
    {
        T t = default;

        try
        {
            HttpClient client = CreateHttpClient(HeaderProperties, timeoutSecond);
            var response = await client.PostAsJsonAsync<V>(requestUrl, v);
            response.EnsureSuccessStatusCode();

            t = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return t;
        }
        catch (Exception ex)
        {
            _logger.LogError($"HttpPost:{requestUrl},body:{v} Error:{ex.ToString()}");

            throw new Exception($"HttpPost:{requestUrl} Error", ex);
        }
    }
    #endregion

    #region CreateHttpClient Methods
    private HttpClient CreateHttpClient(HeaderProperties headerProperties = null, int? timeoutSecond = null)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (headerProperties is not null && headerProperties.Parameters.Any())
        {
            foreach (var headerItem in headerProperties.Parameters)
            {
                if (!httpClient.DefaultRequestHeaders.Contains(headerItem.Key))
                {
                    httpClient.DefaultRequestHeaders.Add(headerItem.Key, headerItem.Value);
                }
            }
        }

        if (timeoutSecond is not null)
        {
            httpClient.Timeout = TimeSpan.FromSeconds(timeoutSecond.Value);
        }

        return httpClient;
    }
    #endregion

    #region Get ResourceStream Method
    private StringContent GenerateStringContent(string requestBody, Dictionary<string, string> dicHeaders)
    {
        var content = new StringContent(requestBody);
        if (dicHeaders != null)
        {
            foreach (var headerItem in dicHeaders)
            {
                content.Headers.Add(headerItem.Key, headerItem.Value);
            }
        }
        return content;
    }
    #endregion
}

