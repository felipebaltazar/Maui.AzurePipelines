using PipelineApproval.Infrastructure.Extensions;

namespace PipelineApproval.Presentation.Views.Pages
{
    public class BaseContentPage : ContentPage
    {
        protected override bool OnBackButtonPressed()
        {
            var parameters = "NavigatingBack".ToNavigationParameters("NavigationMode");

            Application.Current
                .MainPage
                .CurrentPage()
                .RaiseOnNavigatedFrom(parameters);

            return base.OnBackButtonPressed();
        }
    }
}
