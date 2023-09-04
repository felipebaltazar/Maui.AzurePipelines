using PipelineApproval.Presentation.ViewModels.Pages;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class MainPage : BaseContentPage
{
    private MainPageViewModel ViewModel => BindingContext as MainPageViewModel;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if(fEntry.IsVisible)
            fEntry.IsVisible = false;

        await Task.WhenAll(fIcon.ScaleTo(.5, 500, Easing.BounceIn),
                           floatingButton.ScaleTo(.5, 500, Easing.BounceIn));

        await Task.WhenAll(fIcon.ScaleTo(1, 500, Easing.BounceOut),
                           floatingButton.ScaleTo(1, 500, Easing.BounceOut));

        if (fIcon.IsVisible)
        {
            fIcon.IsVisible = false;
            floatingButton.CornerRadius = 0;

            await Task.WhenAll(fIcon.FadeTo(0),
                               floatingButton.ScaleXTo(3.5));

            fEntry.IsVisible = true;
        }
        else
        {
            await Task.WhenAll(fIcon.FadeTo(1),
                               floatingButton.ScaleXTo(1));

            floatingButton.CornerRadius = 32;
            fIcon.IsVisible = true;
        }
    }

    private void fEntry_Completed(object sender, EventArgs e)
    {
        TapGestureRecognizer_Tapped(sender, null);

        _ = Task.Run(ViewModel.GoToPipelineCommandExecuteAsync);
    }
}
