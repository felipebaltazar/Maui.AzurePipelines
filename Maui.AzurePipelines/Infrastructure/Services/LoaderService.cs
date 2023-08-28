using Mopups.Interfaces;
using PipelineApproval.Abstractions;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Presentation.Views.Controls;

namespace PipelineApproval.Infrastructure.Services;

public class LoaderService : ILoaderService
{
    private Lazy<LoaderPopup> _loaderPopUp =
        new Lazy<LoaderPopup>(() => new LoaderPopup());

    private IPopupNavigation _popupNavigation;

    public LoaderService(IPopupNavigation popupNavigation)
    {
        _popupNavigation = popupNavigation;
    }

    public void HideLoading()
    {
        if (_popupNavigation.PopupStack.Any(p => p.GetType() == typeof(LoaderPopup)))
            _popupNavigation.PopAsync().SafeFireAndForget();
    }

    public void ShowLoading()
    {
        if (_popupNavigation.PopupStack.Any(p => p.GetType() == typeof(LoaderPopup)))
            return;

        _popupNavigation.PushAsync(_loaderPopUp.Value).SafeFireAndForget();
    }
}
