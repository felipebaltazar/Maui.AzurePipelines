using System.Globalization;
using System.Reflection;

namespace PipelineApproval.Infrastructure;

public static class ViewModelLocator
{
    public static void LocateViewModel(this BindableObject bindable, IServiceProvider container)
    {
        var view = bindable as Element;
        if (view == null)
            return;

        if (view.BindingContext != null)
            return;

        var viewType = view.GetType();
        var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
        var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
        var viewModelName = string.Format(
            CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

        var viewModelType = Type.GetType(viewModelName);
        if (viewModelType == null)
        {
            return;
        }

        var viewModel = container.GetService(viewModelType);

        view.BindingContext = viewModel;
    }
}
