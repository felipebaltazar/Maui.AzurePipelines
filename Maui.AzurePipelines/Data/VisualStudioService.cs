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
    private readonly ILogger _logger;

    private AccountInfo _accountInfo;

    #endregion

    #region Constructors

    public VisualStudioService(
        IVisualStudioApi visualStudioApi,
        ILogger logger)
    {
        _visualStudioApi = visualStudioApi;
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

    #endregion
}
