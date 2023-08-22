using Android.Icu.Util;
using System.Runtime.CompilerServices;

namespace PipelineApproval;

public partial class MainPage : ContentPage
{
    private MainPageViewModel ViewModel;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = ViewModel = new MainPageViewModel((t,m,c)=> MainThread.InvokeOnMainThreadAsync(()=> DisplayAlert(t,m,c)));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = Task.Run(ViewModel.InitializeAsync);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        _ = Task.Run(ViewModel.OnApperingAsync);
    }
}

