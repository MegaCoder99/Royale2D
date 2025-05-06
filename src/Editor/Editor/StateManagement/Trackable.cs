namespace Editor;

public class Trackable<T>
{
    public T value;
    public string propertyName;

#pragma warning disable CS8604 // Just ignore this instance, T? causes too many issues
    public Trackable(string propertyName) : this(default, propertyName)
#pragma warning restore CS8604 // Possible null reference argument.
    {
    }

    public Trackable(T initialValue, string propertyName)
    {
        value = initialValue;
        this.propertyName = propertyName;
    }

    public void SetValue(
        T value,
        StateComponent parent,
        EditorEvent? changeEvent,
        Action<T, T>? changeCallback,
        Func<T, bool>? validationCallback,
        params string[] depPropNames)
    {
        // Do not validate if context is not initialized yet to prevent spam on startup
        if (!parent.context.IsInitialized())
        {
            validationCallback = null;
        }

        // Possible optimization: if the values are equal, do we even need to run this method? If it does cause a bug, be sure to comment here why
        // If we can optimize, replace usages of if (!selectedMapSection.isDirty) selectedMapSection.isDirty = true;
        parent.context.ApplyUICommitContextIfExists(() =>
        {
            if (!Equals(this.value, value) && validationCallback?.Invoke(value) != false)
            {
                T oldValue = this.value;
                this.value = value;

                // Change callback is responsible for setting its own undo queue entries
                changeCallback?.Invoke(oldValue, value);

                // As with editor event firings too
                if (changeEvent != null)
                {
                    parent.context.FireEvent(changeEvent.Value, parent);
                }

                var invokePropertyChanges = () =>
                {
                    parent.OnPropertyChanged(propertyName);
                    foreach (string depPropName in depPropNames ?? [])
                    {
                        parent.OnPropertyChanged(depPropName);
                    }

                    // Whenever we change a StateComponent trackable, refresh all its properties recursively so that XAML bindings for sections of UI that reference it are all updated
                    if (value is StateComponent stateComponent)
                    {
                        stateComponent.OnPropertyChanged(null);
                    }
                };

                parent.undoManager?.AddUndoNode(
                    new UndoNode(() =>
                    {
                        this.value = oldValue;
                    },
                    () =>
                    {
                        this.value = value;
                    },
                    invokePropertyChanges)
                );
            }
        });
    }
}
