using PipelineApproval.Abstractions;
using PipelineApproval.Models;
using System.Collections.Concurrent;
using System.Net;

namespace PipelineApproval.Infrastructure.Services;

/// <summary>
/// Implementação de factory para otimização do uso de HttpClients, baseado no <see href="https://github.com/aspnet/HttpClientFactory/blob/master/src/Microsoft.Extensions.Http/DefaultHttpClientFactory.cs">DefaultHttpClientFactory</see>
/// </summary>
public sealed class HttpClientFactory : IHttpClientFactory
{
    private readonly Func<string, bool, Lazy<ActiveHandlerTrackingEntry>> _handlerEntryFactory;
    private readonly Func<string, HttpMessageHandler, string, IDictionary<string, string>, bool, Lazy<ActiveHttpClientTrackingEntry>> _clientEntryFactory;

    private readonly ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>> _activeHandlers;
    private readonly ConcurrentDictionary<string, Lazy<ActiveHttpClientTrackingEntry>> _activeHttpClients;

    public HttpClientFactory()
    {
        _activeHandlers = new ConcurrentDictionary<string, Lazy<ActiveHandlerTrackingEntry>>(StringComparer.Ordinal);
        _activeHttpClients = new ConcurrentDictionary<string, Lazy<ActiveHttpClientTrackingEntry>>(StringComparer.Ordinal);

        _handlerEntryFactory = (name, shouldLogHttpResponseContent) =>
        {
            return new Lazy<ActiveHandlerTrackingEntry>(() =>
            {
                return CreateHandlerEntry(name, shouldLogHttpResponseContent);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        };

        _clientEntryFactory = (name, handlerEntry, baseUrl, customHeaders, useBaseUrl) =>
        {
            return new Lazy<ActiveHttpClientTrackingEntry>(() =>
            {
                return CreateHttpClientEntry(name, handlerEntry, baseUrl, customHeaders, useBaseUrl);
            }, LazyThreadSafetyMode.ExecutionAndPublication);
        };
    }


    public HttpClient CreateClient(string name, string baseUrl = null, bool shouldLogHttpResponseContent = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var handler = CreateHandler(name, shouldLogHttpResponseContent);
        var clientEntry = _activeHttpClients.GetOrAdd(name, (str) => _clientEntryFactory(str, handler, baseUrl, null, true)).Value;

        return clientEntry.Client;
    }

    public HttpMessageHandler CreateHandler(string name, bool shouldLogHttpResponseContent = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        var entry = _activeHandlers.GetOrAdd(name, (n) => _handlerEntryFactory.Invoke(n, shouldLogHttpResponseContent)).Value;
        return entry.Handler;
    }

    private static ActiveHttpClientTrackingEntry CreateHttpClientEntry(string name, HttpMessageHandler handler, string baseUrl, IDictionary<string, string> customHeaders = null, bool useBaseUrl = true)
    {
        var baseAddress = new Uri(baseUrl);
        var client = new HttpClient(handler, disposeHandler: false);

        if (useBaseUrl)
            client.BaseAddress = baseAddress;

        else if (customHeaders.Any())
        {
            foreach (var customheader in customHeaders)
            {
                client.DefaultRequestHeaders.Add(customheader.Key, customheader.Value);
            }
        }

        return new ActiveHttpClientTrackingEntry(name, client);
    }

    private ActiveHandlerTrackingEntry CreateHandlerEntry(string name, bool shouldLogHttpResponseContent)
    {
        var httpClientHandler = new HttpClientHandler();

        httpClientHandler.ServerCertificateCustomValidationCallback = (r, x, c, s) => true;
        httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
        httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

#if DEBUG
        var httpClientLoggingHandler = new HttpLoggingHandler(shouldLogHttpResponseContent, httpClientHandler);
        return new ActiveHandlerTrackingEntry(name, httpClientLoggingHandler);
#else
        return new ActiveHandlerTrackingEntry(name, httpClientHandler);
#endif
    }
}
