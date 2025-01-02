using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Management;
using WatcherCore;

namespace TModWatcher;

public static class Program
{
    public static readonly List<int> ProcessIds = [];
    public static Watcher Watcher { get; private set; } = null!;

    /// <summary>
    ///     主程序入口
    /// </summary>
    /// <param name="args">命令参数</param>
    public static void Main(string[] args)
    {
        PrintTModWatcherWelcome();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n输入指令Exit退出程序");

        //获取命令参数
        Dictionary<string, string> arguments = [];
        foreach (string arg in args)
        {
            string[] parts = arg.Split('=');
            if (parts.Length == 2)
                arguments[parts[0]] = parts[1];
        }

        //创建运行配置
        string settingsPath = arguments.GetValueOrDefault("SettingsPath", "WatcherSettings.json");
        WatcherSettings watcherSettings = WatcherSettings.Load(settingsPath);

        //启动监听程序
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n正在启动监听程序......");
        if (HasCsprojOrSlnFile(watcherSettings.WorkPath, out string? assemblyName) && assemblyName != null)
        {
            Watcher = new Watcher(assemblyName, watcherSettings);

            // 查找 TML 进程
            DetectTModLoader();
            // 启用 TML 监听程序
            WatcherTModLoader();

            Task task = Task.Run(Watcher.Start);

            task.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted) return;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(t.Exception);
                },
                TaskContinuationOptions.OnlyOnFaulted
            );
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("监听程序启动成功！");
            Console.WriteLine($"正在监听项目:{watcherSettings.WorkPath}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n{watcherSettings.WorkPath}\n工作目录不是一个有效目录！并没有找到解决方案！");
        }

        //保持主线程运行，输入exit退出
        string? command;
        do command = Console.ReadLine();
        while (command != "exit");
    }

    private static void DetectTModLoader()
    {
        const string targetExecutablePath = @"D:\SteamLibrary\steamapps\common\tModLoader\dotnet\dotnet.exe";

        foreach (Process process in Process.GetProcesses())
        {
            try
            {
                // 获取进程的完整路径
                string? processPath = process.MainModule?.FileName;
                if (!string.Equals(processPath, targetExecutablePath, StringComparison.OrdinalIgnoreCase)) continue;
                Console.WriteLine($"找到 tModLoader 进程: {process.ProcessName}, 进程ID: {process.Id}");
                Watcher.TmlProcess = process;
            }
            catch (Exception e)
            {
                // Console.WriteLine($"无法访问进程 {process.ProcessName} 的信息");
            }
        }

        if (Watcher.TmlProcess != null) return;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("未找到 tModLoader 进程");
        Console.ResetColor();
    }

    [SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
    private static void WatcherTModLoader()
    {
        const string targetProcessPath = @"D:\SteamLibrary\steamapps\common\tModLoader\dotnet\dotnet.exe";

        // 监听进程启动事件
        var processStartQuery = new WqlEventQuery(
            $"SELECT * FROM __InstanceCreationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.ExecutablePath = '{targetProcessPath.Replace(@"\", @"\\")}'"
        );
        var watcherStart = new ManagementEventWatcher(processStartQuery);
        watcherStart.EventArrived += (sender, e) =>
        {
            // 获取启动的进程信息
            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var processId = Convert.ToInt32(targetInstance["ProcessId"]);
            ProcessIds.Add(processId);
            UpdateTModLoaderProcess();
        };

        // 监听进程退出事件
        var processExitQuery = new WqlEventQuery(
            $"SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.ExecutablePath = '{targetProcessPath.Replace(@"\", @"\\")}'"
        );
        var watcherExit = new ManagementEventWatcher(processExitQuery);
        watcherExit.EventArrived += (sender, e) =>
        {
            // 获取启动的进程信息
            var targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            var processId = Convert.ToInt32(targetInstance["ProcessId"]);
            ProcessIds.Remove(processId);
            UpdateTModLoaderProcess();
        };

        // 启动监听
        watcherStart.Start();
        watcherExit.Start();
    }

    private static void UpdateTModLoaderProcess()
    {
        if (ProcessIds.FirstOrDefault() is var processId and > 0)
        {
            if (Watcher.TmlProcess != null && Watcher.TmlProcess.Id == processId) return;
            var process = Process.GetProcessById(processId);
            Watcher.TmlProcess = process;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("成功更新 tModLoader 进程");
            Console.WriteLine($"进程名: {process.ProcessName}");
            Console.WriteLine($"进程ID: {process.Id}");
            Console.WriteLine($"进程启动时间: {process.StartTime}");
            
            var attacher = new ConsoleAttacher(17064);
            // 尝试附加到目标进程的控制台
            if (attacher.Attach())
            {
                // 完成后，记得释放附加的控制台
                attacher.Detach();
            }
        }
        else
        {
            Watcher.TmlProcess = null;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("tModLoader 进程已关闭");
        }

        Console.WriteLine();
        Console.ResetColor();
    }

    /// <summary>
    ///     打印 TModWatcher 欢迎信息
    /// </summary>
    private static void PrintTModWatcherWelcome()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("欢迎使用 ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("TModWatcher！");
        Console.ResetColor();

        Console.Write("这款工具由 ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("TrifingZW ");
        Console.ResetColor();
        Console.WriteLine("开发，旨在帮助你 ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("自动生成资源的C#引用并编译着色器文件");
        Console.ResetColor();
        Console.WriteLine("，让你的 Terraria 模组开发更加轻松！");

        Console.Write("本工具隶属于 ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("万物一心 ");
        Console.ResetColor();
        Console.Write("团队 (");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("https://github.com/ForOne-Club");
        Console.ResetColor();
        Console.WriteLine(")，一个致力于 Terraria 模组开发的团队。");

        Console.Write("如果你有任何问题，欢迎加入我们的QQ群：");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("574904188 ");
        Console.ResetColor();
        Console.Write("或者联系开发者QQ：");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("3077446541");
        Console.ResetColor();

        Console.Write("你也可以访问开发者的GitHub主页：");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("https://github.com/TrifingZW ");
        Console.ResetColor();
        Console.WriteLine("获取更多信息。");

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("TModWatcher ");
        Console.ResetColor();
        Console.Write("基于 ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("MIT开源协议");
        Console.ResetColor();
        Console.Write("，请自觉遵守协议规则。");
    }

    /// <summary>
    ///     判断指定目录下是否存在 .csproj 或 .sln 文件
    /// </summary>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="assemblyName">程序集名称</param>
    /// <returns>文件夹是否存在 .csproj 或 .sln 文件布尔值</returns>
    private static bool HasCsprojOrSlnFile(string directoryPath, out string? assemblyName)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
        {
            assemblyName = null;
            return false;
        }

        // 使用延迟执行来获取文件夹中的所有文件，提高性能
        string? file = Directory.EnumerateFiles(directoryPath)
            .FirstOrDefault(
                file =>
                {
                    string extension = Path.GetExtension(file);
                    return extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)
                           || extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)
                           || extension.Equals(".slnx", StringComparison.OrdinalIgnoreCase);
                }
            );

        assemblyName = file != null ? Path.GetFileNameWithoutExtension(file) : null;

        return assemblyName != null;
    }
}