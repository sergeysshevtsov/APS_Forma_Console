using APS_Forma_Console.Models;

namespace APS_Forma_Console.Navigation;
internal class ConsoleNavigator
{
    private readonly Stack<FolderEntry> folderStack = new();

    public bool CanGoBack => folderStack.Count > 1;
    public FolderEntry? CurrentFolder => folderStack.TryPeek(out var folder) ? folder : null;
    public void EnterFolder(FolderEntry folder) => folderStack.Push(folder);

    public void SetRootFolder(FolderEntry folder)
    {
        folderStack.Clear();
        folderStack.Push(folder);
    }

    public void Back()
    {
        if (CanGoBack)
            folderStack.Pop();
    }
}
