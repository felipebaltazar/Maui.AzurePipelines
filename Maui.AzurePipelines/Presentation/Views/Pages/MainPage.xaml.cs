using AsyncAwaitBestPractices;
using PipelineApproval.Models;
using PipelineApproval.Presentation.ViewModels.Pages;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class MainPage : ContentPage
{
    private MainPageViewModel ViewModel => BindingContext as MainPageViewModel;

    public MainPage()
    {
        InitializeComponent();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        _ = Task.Run(ViewModel.LoadDataAsync);
    }
}
