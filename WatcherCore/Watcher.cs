using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Text;

namespace WatcherCore;

[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
public class Watcher(string filePath, bool snakeCase)
{
    public readonly List<string> FileTypes =
        [".png", ".jpg", ".webp", ".bmp", ".gif", ".mp3", ".wav", ".ogg", ".flac", ".xnb", ".json"];

    public readonly List<string> IgnoreFolders =
        [".git", "bin", "obj", ".idea", "Properties", "Localization", "Resource"];

    public string FilePath { get; } = filePath;
    public string AssemblyName { get; private set; }

    private FileSystemWatcher _fileSystemWatcher;
    private TreeItem _root;

    public StringBuilder Code { get; } = new();

    public void Start()
    {
        _root = new()
        {
            FileName = Path.GetFileName(FilePath),
            FilePath = FilePath,
            Directory = true
        };
        AssemblyName = _root.FileName;

        LoadTree(FilePath, _root);
        GenerateCode();

        if (FilePath != null) _fileSystemWatcher = new(FilePath);
        _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Deleted += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Renamed += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Error += FileSystemWatcherOnError;
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    #region Callback

    private static DateTime _lastEventTime;
    private static string _lastEventInfo;

    private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
    {
        if (IgnoreFolders
            .Select(ignoreFolder => Path.Combine(FilePath, ignoreFolder))
            .Any(folderToIgnore => e.FullPath.StartsWith(folderToIgnore, StringComparison.OrdinalIgnoreCase)))
            return;

        var eventInfo = $"{e.Name} {e.FullPath} {e.ChangeType}";

        if (eventInfo == _lastEventInfo && (DateTime.Now - _lastEventTime).TotalMilliseconds < 500)
            return; // 忽略短时间内的重复事件

        _lastEventTime = DateTime.Now;
        _lastEventInfo = eventInfo;

        //重新监测并生成代码
        _root.CleanChild();
        LoadTree(FilePath, _root);
        GenerateCode();

        var relativePath = Path.GetRelativePath(FilePath, e.FullPath);

        // 打印监测信息
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[Created] {relativePath} at {DateTime.Now}");
                break;
            case WatcherChangeTypes.Deleted:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[Deleted] {relativePath} at {DateTime.Now}");
                break;
            case WatcherChangeTypes.Changed:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Changed] {relativePath} at {DateTime.Now}");
                break;
            case WatcherChangeTypes.Renamed:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[Renamed] {relativePath} at {DateTime.Now}");
                break;
            case WatcherChangeTypes.All:
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[Error] 未知类型:{e}");
                break;
        }
    }

    private static void FileSystemWatcherOnError(object sender, ErrorEventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("监测解决方案时遇到以下错误！");
        Console.WriteLine(sender);
        Console.WriteLine(e.GetException().Message);
    }

    #endregion

    public void Stop()
    {
    }

    private void GenerateCode()
    {
        Code.Clear();
        Code.Append("using System.Diagnostics.CodeAnalysis;\n\n\n");
        Code.Append($"namespace {_root.FileName}.Resource;\n\n");
        Code.Append("[SuppressMessage(\"ReSharper\", \"InconsistentNaming\")]\n");
        Code.Append(new GenerateCode(_root, AssemblyName, snakeCase).Generate());
        var folderPath = Path.Combine(FilePath, "Resource");
        Directory.Delete(folderPath, true);
        Directory.CreateDirectory(folderPath);
        FileStream fileStream = File.Create(Path.Combine(folderPath, "R.cs"));
        using StreamWriter writer = new(fileStream);
        writer.Write(Code);
    }

    private void LoadTree(string path, TreeItem treeItem)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            if (!FileTypes.Contains(Path.GetExtension(file))) continue;
            var relativePath = Path.GetRelativePath(FilePath, file);
            treeItem.CreateChild(Path.GetFileNameWithoutExtension(file), file, relativePath, false);
        }

        foreach (var directory in Directory.GetDirectories(path))
        {
            var relativePath = Path.GetRelativePath(FilePath, directory);
            if (IgnoreFolders.Contains(relativePath)) continue;
            TreeItem dirTreeItem = treeItem.CreateChild(Path.GetFileName(directory), directory, relativePath);
            LoadTree(directory, dirTreeItem);
        }
    }
}