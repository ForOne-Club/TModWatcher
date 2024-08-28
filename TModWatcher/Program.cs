using WatcherCore;

namespace TModWatcher;

public class Program
{
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
        foreach (var arg in args)
        {
            var parts = arg.Split('=');
            if (parts.Length == 2)
                arguments[parts[0]] = parts[1];
        }

        //创建运行配置
        var settingsPath = arguments.GetValueOrDefault("SettingsPath", "WatcherSettings.json");
        WatcherSettings watcherSettings = WatcherSettings.Load(settingsPath);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n正在启动监听程序......");

        //启动监听程序
        if (HasCsprojOrSlnFile(watcherSettings.WorkPath))
        {
            Watcher watcher = new(watcherSettings);
            Task task = Task.Run(watcher.Start);

            task.ContinueWith(t =>
            {
                if (!t.IsFaulted) return;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(t.Exception);
            }, TaskContinuationOptions.OnlyOnFaulted);
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
        string command;
        do
        {
            command = Console.ReadLine();
        } while (command != "exit");
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
    /// <returns>文件夹是否存在 .csproj 或 .sln 文件布尔值</returns>
    public static bool HasCsprojOrSlnFile(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            return false;

        // 获取文件夹中的所有文件
        var files = Directory.GetFiles(directoryPath);

        // 遍历文件，查找 .csproj 或 .sln 文件
        return files.Any(file =>
            Path.GetExtension(file).Equals(".csproj", StringComparison.OrdinalIgnoreCase) ||
            Path.GetExtension(file).Equals(".sln", StringComparison.OrdinalIgnoreCase));
    }
}