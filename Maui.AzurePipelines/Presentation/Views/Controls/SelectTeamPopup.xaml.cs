using AsyncAwaitBestPractices;
using Mopups.Pages;
using Mopups.Services;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Models;

namespace PipelineApproval.Presentation.Views.Controls;

public partial class SelectTeamPopup : PopupPage, ISelectTeamPopup
{
    IList<Team> teams =
            new List<Team>();

    public IList<Team> Teams
    {
        get => teams;
        set
        {
            teams = value;
            BindableLayout.SetItemsSource(this.containerItems, value);
        }
    }

    public Action<Team> OnSelected
    {
        get;
        set;
    }

    public SelectTeamPopup()
	{
		InitializeComponent();
	}

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is not Frame frame)
            return;

        if (e.Parameter is not Team team)
            return;

        OnSelected?.Invoke(team);

        MopupService.Instance.PopAsync()
                             .SafeFireAndForget();
    }

}