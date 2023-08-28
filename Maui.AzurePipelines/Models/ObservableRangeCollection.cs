using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PipelineApproval.Models;

public class ObservableRangeCollection<T> : ObservableCollection<T>
{
    public ObservableRangeCollection()
        : base()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    public void AddRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        if (notificationMode != NotifyCollectionChangedAction.Add && notificationMode != NotifyCollectionChangedAction.Reset)
            throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        var startIndex = Count;

        var itemsAdded = AddArrangeCore(collection);

        if (!itemsAdded)
            return;

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

            return;
        }

        var changedItems = collection is List<T> ? (List<T>)collection : new List<T>(collection);

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Add,
            changedItems: changedItems,
            startingIndex: startIndex);
    }

    public void RemoveRange(IEnumerable<T> collection, NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Reset)
    {
        if (notificationMode != NotifyCollectionChangedAction.Remove && notificationMode != NotifyCollectionChangedAction.Reset)
            throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.", nameof(notificationMode));
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        if (notificationMode == NotifyCollectionChangedAction.Reset)
        {
            var raiseEvents = false;
            foreach (var item in collection)
            {
                Items.Remove(item);
                raiseEvents = true;
            }

            if (raiseEvents)
                RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);

            return;
        }

        var changedItems = new List<T>(collection);
        for (var i = 0; i < changedItems.Count; i++)
        {
            if (!Items.Remove(changedItems[i]))
            {
                changedItems.RemoveAt(i);
                i--;
            }
        }

        if (changedItems.Count == 0)
            return;

        RaiseChangeNotificationEvents(
            action: NotifyCollectionChangedAction.Remove,
            changedItems: changedItems);
    }

    public void Replace(T item) => ReplaceRange(new T[] { item });

    public void ReplaceRange(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException(nameof(collection));

        CheckReentrancy();

        var previouslyEmpty = Items.Count == 0;

        Items.Clear();

        AddArrangeCore(collection);

        var currentlyEmpty = Items.Count == 0;

        if (previouslyEmpty && currentlyEmpty)
            return;

        RaiseChangeNotificationEvents(action: NotifyCollectionChangedAction.Reset);
    }

    private bool AddArrangeCore(IEnumerable<T> collection)
    {
        var itemAdded = false;
        foreach (var item in collection)
        {
            Items.Add(item);
            itemAdded = true;
        }
        return itemAdded;
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T> changedItems = null, int startingIndex = -1)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (changedItems is null)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
        else
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems: changedItems, startingIndex: startingIndex));
    }
}

