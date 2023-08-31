using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Data;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Services;
using Refit;
using System.Diagnostics;
using static PipelineApproval.Infrastructure.Constants.Url;

namespace PipelineApproval.Infrastructure.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddPageAndViewModel<TPage, TViewModel>(
        this IServiceCollection serviceCollection)
        where TPage : class
        where TViewModel : class
    {
        serviceCollection.AddScoped<TPage>();
        serviceCollection.AddScoped<TViewModel>();

        return serviceCollection;
    }

    public static IServiceCollection AddPageStartAndViewModel<TPage, TViewModel>(
        this IServiceCollection serviceCollection)
        where TPage : class, IStartPage
        where TViewModel : class
    {
        serviceCollection.AddScoped<IStartPage, TPage>();
        serviceCollection.AddScoped<TViewModel>();

        return serviceCollection;
    }

    public static IServiceCollection AddAzureApiService(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IVisualStudioService, VisualStudioService>();
        serviceCollection.AddSingleton<IHttpClientFactory, HttpClientFactory>();
        serviceCollection.AddSingleton<IAzureService, AzureService>();

        serviceCollection.AddSingleton((s) =>
            BuildWithFactory<IAzureApi>(s, AZURE_API));

        serviceCollection.AddSingleton((s) =>
            BuildWithFactory<IVisualStudioApi>(s, VISUALSTUDIO_API));

        return serviceCollection;
    }

    private static T BuildWithFactory<T>(IServiceProvider s, string url)
    {
        var factory = s.GetService<IHttpClientFactory>();
        var client = factory.CreateClient(typeof(T).Name, url, Debugger.IsAttached);

        return RestService.For<T>(client);
    }
}
