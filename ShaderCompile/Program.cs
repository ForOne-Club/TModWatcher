namespace ShaderCompile;

public static class Program
{
    public static void Main(string[] args)
    {
        foreach (var arg in args)
        {
            FxCompileEngine fxCompileEngine = new(arg);
            fxCompileEngine.Compile();
        }
    }
}