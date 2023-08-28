using PipelineApproval.Abstractions.Views;

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
}
