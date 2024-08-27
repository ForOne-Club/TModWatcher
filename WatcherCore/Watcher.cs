using System.Diagnostics;
using System.Text;

namespace WatcherCore;

public class Watcher(WatcherSettings watcherSettings)
{
    private FileSystemWatcher _fileSystemWatcher;
    private TreeItem _root;

    public WatcherSettings WatcherSettings { get; set; } = watcherSettings;
    public string WorkPath => WatcherSettings.WorkPath;
    public string AssemblyName => _root.FileName;
    public StringBuilder Code { get; } = new();

    /// <summary>
    ///     启动Watcher
    /// </summary>
    public void Start()
    {
        _root = new()
        {
            FileName = Path.GetFileName(WorkPath),
            FilePath = WorkPath,
            Directory = true
        };

        Console.WriteLine("\n开始进行编译着色器......");
        CompileAllShader(WorkPath);
        Console.WriteLine("全部着色器编译完毕\n");
        LoadFileTree(WorkPath, _root);
        GenerateCode();

        if (WorkPath != null) _fileSystemWatcher = new(WorkPath);
        _fileSystemWatcher.Created += FileSystemWatcher;
        _fileSystemWatcher.Deleted += FileSystemWatcher;
        _fileSystemWatcher.Renamed += FileSystemWatcher;
        _fileSystemWatcher.Changed += FileSystemWatcher;
        _fileSystemWatcher.Error += FileSystemWatcherOnError;
        _fileSystemWatcher.IncludeSubdirectories = true;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }

    /// <summary>
    ///     生成C#代码
    /// </summary>
    private void GenerateCode()
    {
        Code.Clear();
        Code.Append("using System.Diagnostics.CodeAnalysis;\n\n\n");
        Code.Append($"namespace {_root.FileName}.Resource;\n\n");
        Code.Append("[SuppressMessage(\"ReSharper\", \"InconsistentNaming\")]\n");
        Code.Append(new GenerateCode(_root, AssemblyName, WatcherSettings.SnakeCase, WatcherSettings.GenerateExtension).Generate());
        var file = Path.Combine(WorkPath, WatcherSettings.ResourcePath);
        if (Path.GetDirectoryName(file) is { } directory)
            Directory.CreateDirectory(directory);
        FileStream fileStream = File.Create(file);
        using StreamWriter writer = new(fileStream);
        writer.Write(Code);
    }

    /// <summary>
    ///     读取文件树
    /// </summary>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="treeItem">TreeItem</param>
    private void LoadFileTree(string directoryPath, TreeItem treeItem)
    {
        foreach (var filePath in Directory.GetFiles(directoryPath))
        {
            if (!WatcherSettings.FileTypes.Contains(Path.GetExtension(filePath))) continue;
            var relativePath = Path.GetRelativePath(WorkPath, filePath);
            treeItem.CreateChild(Path.GetFileNameWithoutExtension(filePath), filePath, relativePath, false);
        }

        foreach (var directory in Directory.GetDirectories(directoryPath))
        {
            var relativePath = Path.GetRelativePath(WorkPath, directory);
            if (WatcherSettings.IgnoreFolders.Contains(relativePath)) continue;
            TreeItem dirTreeItem = treeItem.CreateChild(Path.GetFileName(directory), directory, relativePath);
            LoadFileTree(directory, dirTreeItem);
        }
    }

    /// <summary>
    ///     编译文件夹内所有着色器
    /// </summary>
    /// <param name="directoryPath">文件夹路径</param>
    private void CompileAllShader(string directoryPath)
    {
        if (WatcherSettings.IgnoreFolders.Contains(Path.GetFileName(directoryPath))) return;

        foreach (var file in Directory.GetFiles(directoryPath))
            if (Path.GetExtension(file) == ".fx")
                CompileShader(file);

        foreach (var directory in Directory.GetDirectories(directoryPath))
            CompileAllShader(directory);
    }

    /// <summary>
    ///     编译单个着色器
    /// </summary>
    /// <param name="filePath">fx文件路径</param>
    private void CompileShader(string filePath)
    {
        // 创建一个新的进程启动信息
        ProcessStartInfo processStartInfo = new()
        {
            FileName = WatcherSettings.ShaderCompile, // 替换为你要调用的外部工具路径
            Arguments = $"\"{filePath}\"" // 替换为要传入的参数
        };

        // 启动进程
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[编译着色器]  ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(Path.GetRelativePath(WorkPath, filePath));

        using Process process = Process.Start(processStartInfo);
        if (process == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("无法启动进程！");
            return;
        }

        // 等待进程完成
        process.WaitForExit();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ---编译完成");
    }

    #region Callback

    private static DateTime _lastEventTime;
    private static string _lastEventInfo;

    private void FileSystemWatcher(object sender, FileSystemEventArgs e)
    {
        //去重
        var eventInfo = e.FullPath;
        if (eventInfo == _lastEventInfo && (DateTime.Now - _lastEventTime).TotalMilliseconds < 500)
            return;
        _lastEventTime = DateTime.Now;
        _lastEventInfo = eventInfo;

        //忽略文件夹
        if (WatcherSettings.IgnoreFolders
            .Select(ignoreFolder => Path.Combine(WorkPath, ignoreFolder))
            .Any(folderToIgnore => e.FullPath.StartsWith(folderToIgnore, StringComparison.OrdinalIgnoreCase)))
            return;

        var relativePath = Path.GetRelativePath(WorkPath, e.FullPath);

        //单独处理fx类型文件
        if (e.FullPath.EndsWith(".fx"))
        {
            switch (e.ChangeType)
            {
                case WatcherChangeTypes.Created:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\n[着色器代码创建]  ");
                    break;
                case WatcherChangeTypes.Changed:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("\n[着色器代码更改]  ");
                    break;
                case WatcherChangeTypes.Renamed:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("\n[着色器代码更名]  ");
                    break;
            }

            if (e.ChangeType != WatcherChangeTypes.Deleted)
            {
                //打印文件名称和时间
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(relativePath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {DateTime.Now}");
                //编译着色器
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("开始重新编译着色器......");
                CompileShader(e.FullPath);
                Console.WriteLine();
            }
        }

        //重新监测并生成代码
        _root.CleanChild();
        LoadFileTree(WorkPath, _root);
        GenerateCode();

        //忽略文件类型
        if (!WatcherSettings.FileTypes.Contains(Path.GetExtension(e.FullPath))) return;

        // 打印监测信息
        switch (e.ChangeType)
        {
            case WatcherChangeTypes.Created:
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[文件创建]  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(relativePath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {DateTime.Now}");
                break;
            case WatcherChangeTypes.Deleted:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("[文件删除]  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(relativePath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {DateTime.Now}");
                break;
            case WatcherChangeTypes.Renamed:
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("[文件重复名]  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(relativePath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {DateTime.Now}");
                break;
            case WatcherChangeTypes.All:
                break;
            case WatcherChangeTypes.Changed:
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("[未知操作]  ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(relativePath);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {DateTime.Now}");
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