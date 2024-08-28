using Microsoft.Xna.Framework.Content.Pipeline;

namespace ShaderCompile;

public class FxBuildLogger : ContentBuildLogger
{
    public override void LogMessage(string message, params object[] messageArgs)
    {
        Console.WriteLine(message, messageArgs);
    }

    public override void LogImportantMessage(string message, params object[] messageArgs)
    {
        Console.WriteLine(message, messageArgs);
    }

    public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
    {
        Console.WriteLine($"Warning: {string.Format(message, messageArgs)}");
    }
}