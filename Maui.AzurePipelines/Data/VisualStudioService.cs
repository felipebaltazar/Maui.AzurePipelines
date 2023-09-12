using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Data;
using PipelineApproval.Infrastructure;
using PipelineApproval.Models;

namespace PipelineApproval;

public class VisualStudioService : BaseMicrosoftService, IVisualStudioService
{
    #region Fields

    private readonly IVisualStudioApi _visualStudioApi;
    private readonly IVsaexApi _vsaexApi;
    private readonly ILogger _logger;

    private AccountInfo _accountInfo;

    #endregion

    #region Constructors

    public VisualStudioService(
        IVisualStudioApi visualStudioApi,
        IVsaexApi vsaexApi,
        ILogger logger)
    {
        _visualStudioApi = visualStudioApi;
        _vsaexApi = vsaexApi;
        _logger = logger;
    }

    #endregion

    #region IVisualStudioService

    public async Task<bool> ValidateAccessTokenAsync(string pat)
    {
        try
        {
            var credentials = FormatToken(pat);
            _accountInfo = await RequestWithRetryPolicy(() =>
                _visualStudioApi.GetAccountInfoAsync(credentials)).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(_accountInfo.id))
                return false;

            await SecureStorage.SetAsync(Constants.Storage.PAT_TOKEN_KEY, pat).ConfigureAwait(false);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao validar PAT", ex);
        }

        return false;
    }

    public async Task<AccountResponseApi> GetOrganizationsAsync()
    {
        try
        {
            var credentials = await GetCredentialsAsync().ConfigureAwait(false);

            return await RequestWithRetryPolicy(() =>
                _visualStudioApi.GetOrganizationsAsync(credentials, _accountInfo.id)).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao buscar organizações", ex);
        }

        return null;
    }

    public async Task<UserEntitlementsReponseApi> GetEntitlementsAsync(string pat, string organization)
    {
        try
        {
            var credentials = FormatToken(pat);

            var result = await RequestWithRetryPolicy(() =>
                _vsaexApi.GetEntitlementsAsync(credentials, organization)).ConfigureAwait(false);

            if(result != null)
                await SecureStorage.SetAsync(Constants.Storage.PAT_TOKEN_KEY, pat).ConfigureAwait(false);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao buscar entitlements", ex);
        }

        return null;
    }

    #endregion
}
