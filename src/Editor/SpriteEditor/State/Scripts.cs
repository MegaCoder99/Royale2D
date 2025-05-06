using Editor;

namespace SpriteEditor;

public partial class State
{
    // For if you need to save everything (to test the save code, or if dirty and enabling save button isn't working due to some bug)
    [Script("fs", "Force Save All")]
    public void ForceSaveAllScript(string[] args)
    {
        ForceSaveAll();
        Prompt.ShowMessage("Done");
    }
}

