using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace Editor;

// Avoid Add(), prefer one bulk Replace() or Append() if adding lots of items in a tight loop
// Otherwise undo nodes/callbacks will be done for each which will be very slow
public class TrackableList<T> : ObservableCollection<T>
{
    private EditorEvent? changeEvent;
    private Action? changeCallback;
    private UndoManager? undoManager => context?.undoManager;
    private StateComponent? stateComponent;
    private EditorContext? context => stateComponent?.context;

    public TrackableList() : base() { }

    public TrackableList(IEnumerable<T> collection, EditorEvent? changeEvent = null, Action? changeCallback = null) : base(collection)
    {
        this.changeEvent = changeEvent;
        this.changeCallback = changeCallback;
    }

    public void SetContextValues(StateComponent stateComponent, EditorEvent? changeEvent, Action? changeCallback)
    {
        this.stateComponent = stateComponent;
        this.changeEvent = changeEvent;
        this.changeCallback = changeCallback;
    }

    public void FireChangeEventIfExists()
    {
        if (changeEvent != null && stateComponent != null)
        {
            context?.FireEvent(changeEvent.Value, stateComponent);
        }
    }

    bool inBulkOperation;

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        if (!inBulkOperation) changeCallback?.Invoke();

        if (inBulkOperation) return;

        FireChangeEventIfExists();

        undoManager?.AddUndoNode(
            new UndoNode(() =>
            {
                base.RemoveItem(index);
            },
            () =>
            {
                base.InsertItem(index, item);
            },
            null)
        );
    }

    protected override void RemoveItem(int index)
    {
        T item = this[index];
        base.RemoveItem(index);
        if (!inBulkOperation) changeCallback?.Invoke();

        if (inBulkOperation) return;

        FireChangeEventIfExists();

        undoManager?.AddUndoNode(
            new UndoNode(() =>
            {
                base.InsertItem(index, item);
            },
            () =>
            {
                base.RemoveItem(index);
            },
            null)
        );
    }

    protected override void SetItem(int index, T item)
    {
        T oldItem = this[index];
        base.SetItem(index, item);
        if (!inBulkOperation) changeCallback?.Invoke();

        if (inBulkOperation) return;

        FireChangeEventIfExists();

        undoManager?.AddUndoNode(
            new UndoNode(() =>
            {
                base.SetItem(index, oldItem);
            },
            () =>
            {
                base.SetItem(index, item);
            },
            null)
        );
    }

    public bool Replace(IEnumerable<T> newItems)
    {
        List<T> oldItems = new List<T>(this);
        List<T> newItemsList = new List<T>(newItems);

        if (oldItems.Count == newItemsList.Count && oldItems.SequenceEqual(newItemsList))
        {
            return false;
        }

        var redoAction = () =>
        {
            inBulkOperation = true;
            Items.Clear();
            foreach (T item in newItems)
            {
                Items.Add(item);
            }
            inBulkOperation = false;
        };

        redoAction.Invoke();
        changeCallback?.Invoke();

        FireChangeEventIfExists();

        undoManager?.AddUndoNode(
            new UndoNode(() =>
            {
                inBulkOperation = true;
                Items.Clear();
                foreach (T item in oldItems)
                {
                    Items.Add(item);
                }
                inBulkOperation = false;
            },
            redoAction,
            null)
        );

        return true;
    }

    public bool Append(IEnumerable<T> newItems)
    {
        if (newItems.Count() == 0) return false;
        var appendedItems = new List<T>(this).Concat(newItems);
        return Replace(appendedItems);
    }

    public new void Clear()
    {
        if (Count > 0)
        {
            Replace([]);
        }
    }

    public void Sort(StateComponent parent)
    {
        if (Count < 2) return;
        List<T> items = new List<T>(this);
        items.Sort();
        Replace(items);

        parent.QueueGenericAction(() =>
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this);
            view.Refresh();
        });
    }
}
