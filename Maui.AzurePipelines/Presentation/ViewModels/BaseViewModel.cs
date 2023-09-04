using Microsoft.Extensions.Logging;
using PipelineApproval.Abstractions;
using PipelineApproval.Abstractions.Views;
using PipelineApproval.Infrastructure.Commands;
using PipelineApproval.Infrastructure.Extensions;
using PipelineApproval.Models;
using System.Runtime.CompilerServices;

namespace PipelineApproval.Presentation.ViewModels;

public abstract class BaseViewModel : ObservableObject, IQueryAttributable
{
    #region Fields

    private bool isBusy;

    private readonly ILazyDependency<INavigationService> _navigationService;

    private readonly ILazyDependency<ILoaderService> _loaderService;

    #endregion

    #region Properties

    public bool IsBusy
    {
        get => isBusy;
        set => SetProperty(ref isBusy, value, onChanged: OnIsBusyChanged);
    }

    public IAsyncCommand NavigateBackCommand =>
        new AsyncCommand(NavigateBackCommandExecuteAsync);

    protected INavigationService NavigationService { get => _navigationService.Value; }

    protected ILoaderService LoaderService { get => _loaderService.Value; }

    protected ILogger Logger { get; }

    public IMainThreadService MainThreadService { get; }

    #endregion

    #region Constructors

    protected BaseViewModel(
        ILazyDependency<ILoaderService> loaderService,
        ILazyDependency<INavigationService> navigationService,
        IMainThreadService mainThreadService,
        ILogger logger) : base(mainThreadService)
    {
        _navigationService = navigationService;
        _loaderService = loaderService;
        Logger = logger;
        MainThreadService = mainThreadService;
    }

    #endregion

    #region Protected Methods

    protected void ExecuteBusyAction(
        Action theBusyAction,
        [CallerMemberName] string memberName = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            theBusyAction?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Busy action process error File: {filePath} | Line: {lineNumber} | Method: {memberName}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected async Task ExecuteBusyActionAsync(
        Func<Task> theBusyAction,
        [CallerMemberName] string memberName = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await theBusyAction?.Invoke();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Busy action process error File: {filePath} | Line: {lineNumber} | Method: {memberName}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected Task ExecuteBusyActionOnNewTaskAsync(
        Func<Task> theBusyAction,
        [CallerMemberName] string memberName = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        return ExecuteBusyActionAsync(
            () => Task.Run(async () => await theBusyAction?.Invoke()),
            memberName,
            filePath,
            lineNumber);
    }

    /// <summary>
    /// This method will fire and forget the action on new Task
    /// </summary>
    /// <param name="theBusyAction"></param>
    /// <param name="memberName"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    protected void ExecuteBusyActionOnNewTask(
        Action theBusyAction,
        [CallerMemberName] string memberName = null,
        [CallerFilePath] string filePath = null,
        [CallerLineNumber] int lineNumber = 0)
    {
        _ = Task.Run(() => ExecuteBusyAction(theBusyAction, memberName, filePath, lineNumber));
    }

    protected Task DisplayAlertAsync(string title, string message, string cancelButton = "Entendi!")
    {
        return NavigationService.PushPopupAsync<IAlertPopup>(p =>
        {
            p.MessageTitle = title;
            p.Message = message;
            p.CancelButton = cancelButton;
        });
    }

    protected virtual Task NavigateBackCommandExecuteAsync()
    {
        var parameters = "NavigatingBack".ToNavigationParameters("NavigationMode");
        return NavigationService.NavigateToAsync("../", parameters);
    }

    private void OnIsBusyChanged()
    {
        if (IsBusy)
            LoaderService.ShowLoading();
        else
            LoaderService.HideLoading();
    }

    #endregion

    #region IQueryAttributable

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query) { }

    #endregion
}
