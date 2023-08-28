using PipelineApproval.Infrastructure;
using Polly;
using Refit;
using System.Net;

namespace PipelineApproval.Models;

public abstract class ObjectWithPolicy
{
    #region Fields

    private readonly IAsyncPolicy _authPolicy;
    private readonly IAsyncPolicy _asyncPolicy;

    #endregion

    #region Constructors

    protected ObjectWithPolicy() : this(null) { }

    protected ObjectWithPolicy(int? numberOfRetries)
    {
        _authPolicy = Policy
            .HandleInner<ApiException>(AuthStatusCodeFilter)
            .RetryAsync(Constants.Policy.MAX_REFRESH_TOKEN_ATTEMPTS, RefreshAuthorizationAsync);

        _asyncPolicy = Policy
            .Handle<ApiException>(StatusCodeFilter)
            .WaitAndRetryAsync(numberOfRetries ?? Constants.Policy.MAX_RETRY_ATTEMPTS, SleepDuration);
    }

    #endregion

    #region Protected Methods


    /// <summary>
    /// Executa a requisição usando uma politica de resiliência
    /// </summary>
    /// <typeparam name="T">Resultado da requisição</typeparam>
    /// <param name="func">Função de requisição</param>
    /// <returns>Resultado da requisição</returns>
    protected async Task<T> RequestWithRetryPolicy<T>(Func<Task<T>> func) =>
        await _asyncPolicy.ExecuteAsync(func).ConfigureAwait(false);

    /// <summary>
    /// Executa a requisição usando uma politica de resiliência e atualização de token
    /// </summary>
    /// <typeparam name="T">Resultado da requisição</typeparam>
    /// <param name="func">Função de requisição</param>
    /// <returns>Resultado da requisição</returns>
    protected async Task<T> RequestWithPolicy<T>(Func<Task<T>> func) =>
        await _asyncPolicy.WrapAsync(_authPolicy).ExecuteAsync(func).ConfigureAwait(false);

    /// <summary>
    /// Executa a requisição usando uma politica de resiliência
    /// </summary>
    /// <param name="func">Função de requisição</param>
    protected async Task RequestWithPolicy<T>(Func<Task> func) =>
        await _asyncPolicy.WrapAsync(_authPolicy).ExecuteAsync(func).ConfigureAwait(false);

    /// <summary>
    /// Executa uma requisição para atualizar token de autorização
    /// </summary>
    /// <param name="error"></param>
    /// <param name="attempt"></param>
    /// <returns></returns>
    protected virtual Task RefreshAuthorizationAsync(Exception error, int attempt) =>
        Task.CompletedTask;

    #endregion

    #region Private Methods

    private static bool StatusCodeFilter(ApiException ex) =>
        ex.StatusCode != HttpStatusCode.NotFound && ex.StatusCode != HttpStatusCode.Forbidden && ex.StatusCode != HttpStatusCode.Unauthorized;

    private static bool AuthStatusCodeFilter(ApiException ex) =>
        ex.StatusCode == HttpStatusCode.Unauthorized;

    private static TimeSpan SleepDuration(int attempt) =>
        TimeSpan.FromSeconds(Math.Pow(2, attempt));

    #endregion
}
