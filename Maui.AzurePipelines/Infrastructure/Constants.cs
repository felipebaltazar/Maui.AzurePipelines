namespace PipelineApproval.Infrastructure;

public static class Constants
{
    public static class Policy
    {
        public const int MAX_REFRESH_TOKEN_ATTEMPTS = 2;

        public const int MAX_RETRY_ATTEMPTS = 3;
    }

    public static class Storage
    {
        public const string PAT_TOKEN_KEY = "personal_access_token";

        public const string USER_ORGANIZATIONS = "user_organizations";
    }

    public static class Navigation
    {
        public const string ACCOUNT_PARAMETER = "AccountInfo";

    }

    public static class Url
    {
        public const string PAT_DOCUMENTATION = "https://learn.microsoft.com/pt-br/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops";

        public const string GITHUB_REPOSIOTRY = "https://github.com/felipebaltazar/Maui.AzurePipelines";

        public const string VISUALSTUDIO_API = "https://app.vssps.visualstudio.com";

        public const string AZURE_API = "https://dev.azure.com";
    }
}
