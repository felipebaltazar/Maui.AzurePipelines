using PipelineApproval.Presentation.ViewModels.Pages;

namespace PipelineApproval.Presentation.Views.Pages;

public partial class PipelineDetailsPage : BaseContentPage
{
    public PipelineDetailsPage()
    {
        InitializeComponent();
    }

    private void VirtualListView_OnSelectedItemsChanged(object sender, SelectedItemsChangedEventArgs e)
    {
        if (sender is VirtualListView listView)
        {
            if (BindingContext is PipelineDetailsPageViewModel vm)
            {
                var item = vm.GetRecordAt(listView.SelectedItem?.ItemIndex ?? -1);
                _ = Task.Run(() =>
                {
                    vm.SelectedRecord = item;
                });
            }

            listView.SelectedItem = null;


        }
    }
}