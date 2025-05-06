# I. Overview

*Before reading this doc, read StateManagement.md for some background info on the state management code structure.*

The MVVM pattern touted by WPF is followed to some degree. We have "Model" classes that represent the asset data stored on disk, which are separate from the "StateComponents" (view models) that are used by the WPF XAML layer and undo/redo system. This cleanly separates the contracts that are saved to/read from disk into a standalone "class" (actually C# 9 records) making it easier to specify and understand what is saved to disk, and reuse the models for non-view model purposes like bulk import/export processes and scripts, and even potentially  by an external C# game engine.

Some StateComponents directly map to a model class, but not all of them do (as some state components are organizational containers or map to specific XAML sections or controls). The ones that do inherit from `StateComponentWithModel<YourModelClass>` and have code that maps from and to the model for serialization and deserialization to/from the files stored on disk. MVVM and seperation of concerns is great, but forces boilerplate and copy-pasted "mapper" code and risk of error and getting things out of sync. To combat this, at the top of each `StateComponentWithModel` class, there is a streamlined code pattern/structure that attempts to reduce such chance of error, so follow the structure/pattern of existing ones when making new ones. More on this pattern later. Model classes are generally C# records, immutable with a primary constructor that reduces chance of error since adding or removing a field automatically updates the constructor.

The reason view models are called state components in this codebase is twofold. First, I don't really like the term "ViewModel" because it has the word "Model" in it which is confusing. "ViewModelWithModel" will explode your brain. "Controller" was a possibility but it sounds web API related and this is a rich client with no service component. I took a page out of modern libraries like React/Redux and use the term "State". I call the top level viewmodel object "State" and the ViewModel base class "StateComponent" since the top-level State is composed of them. Also, they do more than just act as a "view model" for XAML bindings, they hold undo'able and redo-able "trackable" fields and have a reference to the editor context for undo/redo state operations on their fields.

Finally, a note on justification of this pattern and the additional complexity it introduces: at first, I did NOT have a separate model layer. So everything was done in the state components, including serialization/deserialization. There were some growing pain points. This meant I had to put `[JsonIgnore]` everywhere in state component classes to ignore UI-only fields when writing to disk, and it could be hard to isolate what was saved to disk vs just used by UI. It was also hard to re-use the models for non-UI operations like bulk import/export processes since they would get undesirable undo/redo context forced onto them which could cause perf issues and errors outside the editor UI context.

# II. Example

## StateComponentWithModel (#region model)

```
public class Sprite : StateComponentWithModel<SpriteModel>
{
    #region model
    public string name { get => TrGetD(model.name); set => TrSet(value); }
    public TrackableList<Frame> frames { get => TrListGet(model.frames.Select(f => new Frame(context, f))); init => TrListSet(value); }
    public TrackableList<Hitbox> hitboxes { get => TrListGet(model.hitboxes.Select(h => new Hitbox(context, h))); init => TrListSet(value); }
    public int loopStartFrame { get => TrGetD(model.loopStartFrame); set => TrSet(value); }
    public string alignment { get => TrGetD(model.alignment); set => TrSet(value); }
    public string wrapMode { get => TrGetD(model.wrapMode); set => TrSet(value); }
    public string spritesheetName { get => TrGetD(model.spritesheetName); set => TrSet(value); }
    
    public SpriteModel ToModel()
    {
        return new SpriteModel(
            name,
            spritesheetName,
            frames.SelectList(f => f.ToModel()),
            hitboxes.SelectList(h => h.ToModel()),
            loopStartFrame,
            alignment,
            wrapMode
        );
    }
    #endregion

    // Rest of class with trackable fields, etc...
```

### Remarks
- Put everything that maps to model class in a `#region model` at the top. This makes it easy to see the model interation, and isolates mapping code in one place.
- Constructors (covered below) should NOT go here. Generally, constructors should NOT be initializing properties from model (unless they are not trackables, which is uncommon.) Instead, assign them as the default value in `TrGetD` (used to initialize default values which is what the D stands for) or `TrListGet` helpers.
  - This is IMO less error prone. Were we to assign them in constructor, it's less visible and further away from these model property definitions, and thus more likely someone forgets to update the constructor with the field mapping when modifying the model.
- `ToModel()` will return the model using the model's primary constructor. The primary constructor syntax of records is great because if you add, change or remove a model field in one place, the complier forces you to update all places that construct that record, like this `ToModel()` function. Hence providing a compile time safety net against forgetting to update all "map to model" code. This is why you should use primary constructors for models and NOT have a default empty constructor, which is error prone.
- Notice how for nested model classes in lists like Frame, Hitbox, and whatnot, you'll have to do a nested mapping with Select linq queries and whatnot.
- `ToModel()` is generally only called when you need to save to disk. The model is generally not updated in real time as you update state in your app and undo/redo and such. There isn't really a need and it would be complex and slow with the undo/redo and trackable system.
- Note that while there is a `model` variable provided in `StateComponentWithModel`, it is only set once in constructor for use in `TrGetD` intialization and such and generally not updated (unless your specific class has custom code that tries to keep it up-to-date, which isn't something I really found a need for. Models are mostly useful for saving to disk, and maybe sometimes for simple reusable validation that doesn't rely on much additional context outside the model properties themselves.)

## StateComponentWithModel (constructors)

```
  public Sprite(EditorContext context, SpriteModel model) : base(context, model)
  {
      selectedFrame = frames.SafeGet(0);
  }

  public Sprite(EditorContext context, string name, string spritesheetName) : 
      this(context, SpriteModel.New(name, spritesheetName))
  {

  }
```

### Remarks
- Always have a "base" constructor that passes in two parameters, the context and the model. This constructor should run any code that MUST happen on each creation, such as in this case, setting the selected frame to the first frame since selected frame can't be null.
- All other constructors should reference the base constructor with `this()` syntax, ensuring they get the necessary base initializations.
- Other constructors with a subset of parameters can be useful if you don't need to pass in the world to create a usable state component. You should define a similar factory method (not constructor) in your model, and pass it in as the model parameter to the base constructor with `this` syntax. more on that in model section below.

## Model

```
public record SpriteModel(
    string name,            // Not saved to disk, but in the model class to simplify runtime code
    string spritesheetName, // Will not be set in engine export if sprite is packaged during export.
    List<FrameModel> frames,
    List<HitboxModel> hitboxes,
    int loopStartFrame,
    string alignment,
    string wrapMode
)
{
    public static SpriteModel New(string name, string spritesheetName) =>
        new SpriteModel(name, spritesheetName, [], [], 0, Alignment.Center, WrapMode.Loop);
}
```

### Remarks
- We use a record for several reasons. First, the primary constructor syntax is very concise and removes both copy paste and error-prone situations where you can add/rename/remove a field in one place but forget to update it in another. This is a godsend in M<=>VM mapper code where you have to otherwise copy paste a lot and things can get out of sync.
- Record immutability can also sometimes come in handy in preventing accidental errors in model serialization/deserialization and import/export pipelines.
  - I am aware of ImmutableList but decided not to use it since it adds complexity due to more conversion of list types required everywhere and is potentially a perf concern for large lists. Instead a standard List is used.
- If you need more constructors than the primary one with all the fields, specify them as static factory methods in the record, not constructors. Otherwise System.Text.Json freaks out and throws. See gotcha section further down for details on why this is needed.
- Avoid empty model constructors that just default everything to null/empty and potentially invalid values. This bypasses all creation validation and is risky and error prone if someone stumbles upon and decides to use the default constructor when they aren't supposed to because it makes things easy. All constructors need to create valid objects that the editor can work with properly.

# III. Best Practices And Gotchas

A lot were covered above in the example, so will only go over ones not covered, or elaborate existing remarks in more detail.

## System.Text.Json will set missing fields as null when deserializing to models

This can cause non-nullable reference types like strings, lists, etc. to be null, which is unexpected. There doesn't seem to be a way around this at the System.Text.Json level. One option is to default model strings, lists, etc. to "", [], etc. But doing so with the primary record constructor pattern isn't really possible without having to copy-paste the same field definition twice and risk getting them out of sync. 

As such, when reading model fields, prepare for the possibility they are null, if they are strings (since strings are configured in the serializer settings not written to disk if they are empty) or if they are new fields that are lists or reference types that existing assets have not migrated to yet. `TrGetD` will handle the string case, automatically turning a `null` into a `""`. `TrListGet` handles the list case as well, turning it into `[]`, but it will not necessarily prevent null ref errors if you don't check yourself when referencing it before passing it in. For example, use a `?` in those case, such as  `TrListGet(model.hitboxes?.Select(...` if you are in a situation where hitboxes could not be set in JSON for some asset files to avoid a null ref. Same applies for model objects that aren't in a list but individually referenced; check for null before it enters `TrGetD` as necessary.

## Multiple record constructors cause System.Text.Json issues; use static factory methods instead

You would need to have `[JsonConstructor]` on the constructor you'd want to use. Unfortunately with primary constructor syntax, you can't set this on the primary constructor. See issue here: https://github.com/dotnet/runtime/issues/45373. As previously mentioned, primary constructors are highly useful for validation and preventing copy-paste of model properties. So a workaround to keep primary constructor is to use static factory methods inside the record if you need multiple constructors.