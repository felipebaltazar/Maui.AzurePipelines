using PipelineApproval.Infrastructure;
using PipelineApproval.Models;
using System.Text;

namespace PipelineApproval;

public abstract class BaseMicrosoftService : ObjectWithPolicy
{
    private string _credentials;

    protected async ValueTask<string> GetCredentialsAsync()
    {
        if (_credentials is null)
        {
            var pat = await SecureStorage.GetAsync(Constants.Storage.PAT_TOKEN_KEY);
            _credentials = FormatToken(pat);
        }

        return _credentials;
    }

    protected string FormatToken(string pat)
    {
        var auth = string.Format("{0}:{1}", string.Empty, pat);
        var authBytes = Encoding.ASCII.GetBytes(auth);

        return "Basic " + Convert.ToBase64String(authBytes);
    }
}
