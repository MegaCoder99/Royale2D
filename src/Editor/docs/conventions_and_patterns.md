## Avoid saving references to model file name/path in the model JSON

For example, don't have a "name" field in a sprite JSON that is an exact copy of the file name and must be kept in constant sync. It should be represented by its file system name and path to avoid repeating yourself and another place you have to change if you want to rename the model in the file system or move it to a nested folder.

This does make JSON deserialization more complex as a downside. See the pattern below for handling that complexity in a predicable way.

## Models saved on disk to have a formulaic constructor pattern

Your model class should have a "CreateFromEditor" static factory method. This is to be called by code that manually creates a model class from the editor and NOT by the deserializer. Do not use actual constructors because System.Text.JSON will complain since we need FilePath as a parameter to the creation method and it won't pass that in for us. The reason we pass a relative file path object instead of just a file name is because we want to abstract the model's path in the top-level workspace folder it lives in, in case we want to move JSON model files to nested inner folders.

```
public static Sprite CreateFromEditor(FilePath relativeFilePath)
{
    var sprite = new Sprite();
    sprite.Init(relativeFilePath);
    return sprite;
}
```

Right below, define an `Init` function that takes in the relative file path. This will be called by both deserialization code and your static factory method "constructor".

```
public void Init(FilePath relativeFilePath)
{
    relativeFilePath.AssertIsRelative();    // Helpful runtime assert to check we're doing the right thing
    this.relativeFilePath = relativeFilePath;
}
```

Finally, in all places that deserialize this model from JSON, be sure to call Init afterwards:

```
private static void InitSprites()
{
    List<FilePath> spritefiles = spritesPath.GetFiles(true, "json");
    List<Sprite> sprites = new List<Sprite>();
    foreach (FilePath spriteFile in spritefiles)
    {
        string spriteJson = File.ReadAllText(spriteFile.fullPath);
        var sprite = JsonHelpers.DeserializeJson<Sprite>(spriteJson);
        sprite.Init(spriteFile.GetRelativeFilePath(spritesPath));   // <= CALL INIT HERE, RIGHT AFTER DeserializeJson() CALL
        sprites.Add(sprite);
    }

    SpriteEditorWorkspace.sprites = sprites;
}
```

## Avoid supporting operations that rename/delete files on disk

Right now the editors don't support deletes/renames of sprites or map sections in the UI; Windows Explorer, cmd/powershell, etc. is great for this and already exists for us to leverage. We don't have to rebuild this functionality as it provides little and has a ton of caveats/complexity involving undo. Generally as a pattern, file names and paths are not referenced in their JSON anywhere, so renaming sprites/map sections from Windows Explorer is ok*. Just don't do it while you are using the editor. If you must, reload the editor when you are done making Explorer changes to avoid potentially weird behavior especially with undo queues and whatnot.

**In some cases you may have to rename other places, for example if renaming or moving spritesheets you need to modify sprite JSON references to that spritesheet. Fear not, this is why the asset files are JSON and not some cryptic binary format! Just open your JSON files in your favorite text editor and search/replace manually. This isn't a common case anyway, so let's not try to have the editor do everything.*

You might be wondering, why have sprites/map sections/etc in separate files. We'd avoid this issue if they were all in one big file... but then you have one giant file that's hard to read and maintain when you do need to go in there. Version control is also harder with one giant file as opposed to separate streamlined ones. Tileset is an exception to this rule though because there are a massive amount of them and each tile is quite small and simple in its data.

File creation operations are fair game. For those, there is much greater value in creation from editor because it avoids annoying manual creation of JSON. See below on some caveats of this functionality.

## Operations that create new files on disk should not be undo'able and should save immediately

Were file add operations undo'able, it would complicate things by forcing us to delete from disk when undo'ing and there isn't a concept of "removed disk file" dirty in the editor right now (it would be pretty complex, error prone and risky.)

For this reason, any addition of entities represented by disk files (map sections, sprites, etc) should not be undo'able. Instead they should write to disk directly, doing a Save() automatically. I am aware we could build some "virtual filesystem dirty change tracking" in the editor. However, this is a complex feature for not a lot of gain and one of the tenants of the editor is simplicity. As of right now I want to keep things simple, as it makes the code easy to go into and change and tweak to specific needs. Plus, if this feature is screwed up, it could result in bugs involving permanent accidental file deletion which is not fun.

## Exported JSON assets should not have entire objects duplicated

If you need relationships between assets, reference them via a single id/name field instead. Don't duplicate entire objects because this will mess up game engine deserialization. Have the game engine handle parsing and deserializing and setting up shared object references.