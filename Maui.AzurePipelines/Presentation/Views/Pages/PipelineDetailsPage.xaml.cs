using PipelineApproval.Presentation.ViewModels.Pages;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class PipelineDetailsPage : BaseContentPage
{
    public PipelineDetailsPage()
    {
        InitializeComponent();
    }

    private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var item = e.SelectedItem as Record;

        if (sender is ListView listView)
        {
            listView.SelectedItem = null;
        }

        if (BindingContext is PipelineDetailsPageViewModel vm)
        {
            vm.SelectedRecord = item;
        }
    }
}