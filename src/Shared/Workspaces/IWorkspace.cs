namespace Shared;

public interface IWorkspace
{
    bool IsValid(out string errorMessage);
    void LoadFromDisk(bool isPackaged);
    public FolderPath baseFolderPath { get; }
}
