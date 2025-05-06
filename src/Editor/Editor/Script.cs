namespace Editor;

public class Script
{
    public string description { get; set; }
    public Action<string[]> action { get; set; }

    public Script(string description, Action<string[]> action)
    {
        this.description = description;
        this.action = action;
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class ScriptAttribute : Attribute
{
    public string command { get; } = "";
    public string description { get; } = "";

    public ScriptAttribute()
    {
    }

    public ScriptAttribute(string command, string description = "")
    {
        this.command = command;
        this.description = description;
    }
}
