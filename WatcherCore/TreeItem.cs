namespace WatcherCore;

public class TreeItem
{
    public List<TreeItem> TreeItems { get; } = [];

    public string FileName { get; init; }
    public string FilePath { get; init; }
    public string RelativePath { get; init; }
    public bool Directory { get; init; }
    public int ChildCount => TreeItems.Count;

    public TreeItem CreateChild(string name, string path, string relativePath, bool directory = true)
    {
        TreeItem treeItem = new()
        {
            FileName = name,
            FilePath = path,
            RelativePath = relativePath,
            Directory = directory
        };
        TreeItems.Add(treeItem);
        return treeItem;
    }

    public void RemoveChild(TreeItem treeItem)
    {
        TreeItems.Remove(treeItem);
    }

    public void RemoveChild(int index)
    {
        TreeItems.RemoveAt(index);
    }

    public void CleanChild()
    {
        TreeItems.Clear();
    }

    public bool HasFile()
    {
        var hasFile = false;
        Ergodic(this, item =>
        {
            if (item.Directory) return false;
            hasFile = true;
            return true;
        });
        return hasFile;
    }

    public static void Ergodic(TreeItem treeItem, Func<TreeItem, bool> action)
    {
        foreach (TreeItem item in treeItem.TreeItems)
        {
            if (action(item)) return;
            Ergodic(item, action);
        }
    }
}