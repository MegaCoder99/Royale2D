using Shared;
using System.Reflection;
using System.Windows.Input;

namespace Editor;

public class ScriptManager
{
    public Dictionary<string, Script> commandToScript = new Dictionary<string, Script>();
    public Dictionary<Key, Script> hotkeyToScript = new Dictionary<Key, Script>();

    public ScriptManager(object obj)
    {
        var scriptMethods = obj.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)
            .Where(m => m.GetCustomAttributes(typeof(ScriptAttribute), false).Length > 0)
            .ToArray();

        foreach (MethodInfo method in scriptMethods)
        {
            var attribute = (ScriptAttribute)method.GetCustomAttributes(typeof(ScriptAttribute), false).First();
            Action<string[]> action = (Action<string[]>)Delegate.CreateDelegate(typeof(Action<string[]>), obj, method);

            var script = new Script(attribute.description, action);

            if (!string.IsNullOrEmpty(attribute.command))
            {
                commandToScript.Add(attribute.command, script);
            }
        }
    }

    public void RunScript(string text)
    {
        string[] pieces = text.Split(' ');
        string scriptText = pieces[0];
        string[] args = pieces.Skip(1).ToArray();

        if (scriptText.Unset() || !commandToScript.ContainsKey(scriptText))
        {
            Prompt.ShowError("Invalid script command: " + scriptText);
            return;
        }

        Script script = commandToScript[scriptText];
        try
        {
            script.action.Invoke(args);
        }
#if !DEBUG
        catch (Exception ex)
        {
            Prompt.ShowError("Error running script " + script.description + ":\n" + ex.Message);
        }
#else
        finally { }
#endif
    }
}
