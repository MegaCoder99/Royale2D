using Shared;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Editor;

// "[CallerMemberName] string? propertyName = null" parameters should NEVER be passed in manually or you will mess up the trackable system

public class StateComponent : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged(string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // object because we can't "genericize" generic types
    protected Dictionary<string, object> trackables = new();

    public EditorContext context;
    public UndoManager? undoManager => context.undoManager;

    public StateComponent(EditorContext context)
    {
        this.context = context;
    }

    public Func<string, bool> ValidateStringIsSet([CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentException("propertyName cannot be null");
        return (string stringValue) =>
        {
            if (!stringValue.IsSet())
            {
                Prompt.ShowError($"{propertyName} cannot be empty");
                return false;
            }
            return true;
        };
    }

    public T TrGet<T>([CallerMemberName] string? propertyName = null)
    {
        return TrGetD(default(T), propertyName)!;
    }

    // For defaulting to an initial value (D stands for Default)
    public T TrGetD<T>(T? initialValue, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentException("propertyName cannot be null");
        Trackable<T> trackable = GetOrCreateTrackable(propertyName, initialValue);
        return trackable.value;
    }

    public void TrSet<T>(T value, string[]? depPropNames = null, [CallerMemberName] string? propertyName = null)
    {
        TrSetAll(value, depPropNames, propertyName: propertyName);
    }

    // For editor events (E stands for event)
    public void TrSetE<T>(T value, EditorEvent? editorEvent = null, [CallerMemberName] string? propertyName = null)
    {
        TrSetAll(value, editorEvent: editorEvent, propertyName: propertyName);
    }

    // For on change callbacks (C Stands for Change/Callback)
    public void TrSetC<T>(T value, Action<T, T> onChangeCallback, [CallerMemberName] string? propertyName = null)
    {
        TrSetAll(value, onChangeCallback: onChangeCallback, propertyName: propertyName);
    }

    // For validation callbacks. Value will not be changed if the callback returns false. (V stands for Validate)
    public void TrSetV<T>(T value, Func<T, bool> validationCallback, [CallerMemberName] string? propertyName = null)
    {
        TrSetAll(value, validationCallback: validationCallback, propertyName: propertyName);
    }

    // For if you need a combination of the above
    public void TrSetAll<T>(
        T value,
        string[]? depPropNames = null,
        EditorEvent? editorEvent = null,
        Action<T, T>? onChangeCallback = null,
        Func<T, bool>? validationCallback = null,
        [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentException("propertyName cannot be null");
        // Force the get property to be invoked before calling the set.
        GetType().GetProperty(propertyName)!.GetValue(this);
        Trackable<T> trackable = GetOrCreateTrackable<T>(propertyName, default);
        trackable.SetValue(value, this, editorEvent, onChangeCallback, validationCallback, depPropNames ?? []);
    }

    private Trackable<T> GetOrCreateTrackable<T>(string propertyName, T? initialValue)
    {
        if (!trackables.ContainsKey(propertyName))
        {
            // JSON deserializer + record constructor setup results in potentially null strings that bypass compiler, so set them to empty for additional safety
            if (typeof(T) == typeof(string) && initialValue == null)
            {
                trackables[propertyName] = new Trackable<string>("", propertyName);
            }
            else
            {
#pragma warning disable CS8604 // Possible null reference argument.
                trackables[propertyName] = new Trackable<T>(initialValue, propertyName);
#pragma warning restore CS8604 // Possible null reference argument.
            }
        }
        return (trackables[propertyName] as Trackable<T>)!;
    }

    // Unlike with the "regular" TrSet/Sets, the list one has the action callbacks in the Get.
    // This is because consumers performing operations on lists are not changing the list reference itself. They're just getting the reference and calling method on it
    // Hence, it makes sense to plug in any contexts in Get and not Set
    public TrackableList<T> TrListGet<T>(IEnumerable<T>? defaultValue = null, Action? onChangeCallback = null, EditorEvent? changeEvent = null, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentException("propertyName cannot be null");
        if (!trackables.ContainsKey(propertyName))
        {
            trackables[propertyName] = new TrackableList<T>(defaultValue ?? []);
        }
        var retVal = (trackables[propertyName] as TrackableList<T>)!;
        retVal.SetContextValues(this, changeEvent, onChangeCallback);
        return retVal;
    }

    public void TrListSet<T>(TrackableList<T> value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentException("propertyName cannot be null");
        trackables[propertyName] = value;
    }

    // For the methods below: these are just helpers that wrap the calls in an undo node, ensuring they are undo'able.
    // Like with AddUndoNode, these should only be called while in a commit context or they will not work
    // You would call these in a commit context if you want to invoke some undo'able code block that isn't a Trackable field change
    // or one of its associated PropertyChanged's firing. You'd call these in TrSetC callbacks and editor event handler code, since
    // those are going to be in a commit context automatically, assuming all rules are followed.
    // Note that these "Queue" methods always also instantaneously apply the action in addition to "queueing" it up

    public void QueueOnPropertyChanged(string propertyName)
    {
        undoManager?.AddUndoNode(null, null, () => OnPropertyChanged(propertyName));
    }

    // IMPORTANT: requirements for action (these apply to the ENTIRE call stack):
    // - It must not change any trackable property.
    // - It must not call any EditorContext/UndoManager mutation methods.
    // In general, keep it simple. It should be restricted to side effects of changing a simple non-trackable property or something.
    // Good examples include non-trackable, cached data structures that should happen on each change of some property, changing some non-trackable UI only flag, etc.
    public void QueueGenericAction(Action action)
    {
        undoManager?.AddUndoNode(null, null, action);
    }
}

public class StateComponentWithModel<TModel> : StateComponent
{
    // Won't actually get updated. This is just for initialization purposes / setting default field values to deserialized model
    public TModel model;

    public StateComponentWithModel(EditorContext context, TModel model) : base(context)
    {
        this.model = model;
    }
}