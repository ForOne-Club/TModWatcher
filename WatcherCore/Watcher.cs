using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace WatcherCore;

[SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
public class Watcher(string filePath, bool snakeCase, bool generateExtension)
{
    public readonly List<string> FileTypes =
        [".png", ".jpg", ".webp", ".bmp", ".gif", ".mp3", ".wav", ".ogg", ".flac", ".xnb", ".json"];

    public readonly List<string> IgnoreFolders =
        [".git", "bin", "obj", ".idea", "Properties", "Localization", "Resource"];

    public static readonly string ShaderCompile = "ShaderCompile/ShaderCompile.exe";

    private FileSystemWatcher _fileSystemWatcher;
    private TreeItem _root;

    public string FilePath { get; } = filePath;
    public string AssemblyName { get; private set; }

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

        Console.WriteLine("\n开始进行编译着色器");
        CompileShader(FilePath);
        Console.WriteLine("全部着色器编译完毕\n");
        LoadTree(FilePath, _root);
        GenerateCode();

        if (FilePath != null) _fileSystemWatcher = new(FilePath);
        _fileSystemWatcher.Created += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Deleted += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Renamed += FileSystemWatcherOnCreated;
        _fileSystemWatcher.Changed += (_, e) =>
        {
            //编译着色器
            if (e.FullPath.EndsWith(".fx"))
                CompileFx(e.FullPath);
        };
        _fileSystemWatcher.Error += FileSystemWatcherOnError;
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
    }

    private void GenerateCode()
    {
        Code.Clear();
        Code.Append("using System.Diagnostics.CodeAnalysis;\n\n\n");
        Code.Append($"namespace {_root.FileName}.Resource;\n\n");
        Code.Append("[SuppressMessage(\"ReSharper\", \"InconsistentNaming\")]\n");
        Code.Append(new GenerateCode(_root, AssemblyName, snakeCase, generateExtension).Generate());
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

    private void CompileShader(string path)
    {
        foreach (var file in Directory.GetFiles(path))
            if (Path.GetExtension(file) == ".fx")
                CompileFx(file);

        foreach (var directory in Directory.GetDirectories(path))
            CompileShader(directory);
    }

    private void CompileFx(string path)
    {
        // 创建一个新的进程启动信息
        ProcessStartInfo processStartInfo = new()
        {
            FileName = ShaderCompile, // 替换为你要调用的外部工具路径
            Arguments = $"\"{path}\"", // 替换为要传入的参数
        };

        // 启动进程
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"编译着色器:{Path.GetRelativePath(FilePath, path)}");

        using Process process = Process.Start(processStartInfo);
        if (process == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("无法启动进程！");
            return;
        }

        // 等待进程完成
        process.WaitForExit();
        Console.WriteLine($"编译完成");
    }

    #region Callback

    private static DateTime _lastEventTime;
    private static string _lastEventInfo;

    private void FileSystemWatcherOnCreated(object sender, FileSystemEventArgs e)
    {
        var eventInfo = $"{e.Name} {e.FullPath} {e.ChangeType}";

        if (eventInfo == _lastEventInfo && (DateTime.Now - _lastEventTime).TotalMilliseconds < 500)
            return; // 忽略短时间内的重复事件

        _lastEventTime = DateTime.Now;
        _lastEventInfo = eventInfo;

        //忽略文件夹
        if (IgnoreFolders
            .Select(ignoreFolder => Path.Combine(FilePath, ignoreFolder))
            .Any(folderToIgnore => e.FullPath.StartsWith(folderToIgnore, StringComparison.OrdinalIgnoreCase)))
            return;

        //编译着色器
        if (e.FullPath.EndsWith(".fx"))
            CompileFx(e.FullPath);

        //忽略文件类型
        if (!FileTypes.Contains(Path.GetExtension(e.FullPath))) return;

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
                Console.WriteLine($"[文件创建] {relativePath} AT {DateTime.Now}");
                break;
            case WatcherChangeTypes.Deleted:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[文件删除] {relativePath} AT {DateTime.Now}");
                break;
            case WatcherChangeTypes.Changed:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[文件改变] {relativePath} AT {DateTime.Now}");
                break;
            case WatcherChangeTypes.Renamed:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[文件更名] {relativePath} AT {DateTime.Now}");
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
}