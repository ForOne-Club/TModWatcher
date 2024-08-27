using System.Text.Json;

namespace WatcherCore;

public class WatcherSettings
{
    public string WorkPath { get; set; } = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
    public string ShaderCompile { get; set; } = "ShaderCompile/ShaderCompile.exe";
    public bool SnakeCase { get; set; } = true;
    public bool GenerateExtension { get; set; } = true;
    public string ResourcePath { get; set; } = "Resource/R.cs";
    public List<string> FileTypes { get; set; } = [".png", ".jpg", ".webp", ".bmp", ".gif", ".mp3", ".wav", ".ogg", ".flac", ".xnb"];
    public List<string> IgnoreFolders { get; set; } = [".git", ".idea", ".vs", "bin", "obj", "Properties", "Localization", "Resource"];

    public void Save(string path)
    {
        var jsonString = JsonSerializer.Serialize(this, SerializeOnlyContext.Default.WatcherSettings);
        File.WriteAllText(path, jsonString);
    }

    public static WatcherSettings Load(string path)
    {
        WatcherSettings watcherSettings = new();
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[读取配置文件]  ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(path);
        if (File.Exists(path))
            try
            {
                watcherSettings = JsonSerializer.Deserialize(File.ReadAllText(path), SerializeOnlyContext.Default.WatcherSettings);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  ---读取配置文件完成");
                return watcherSettings;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("  ---读取配置文件错误");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("错误跟踪:");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{ex.Message}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("堆栈跟踪:");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"{ex.StackTrace}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("将生成默认配置并使用");
                return watcherSettings;
            }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  ---配置文件不存在");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("将生成默认配置并使用。");
        watcherSettings.Save(path);
        return watcherSettings;
    }
}