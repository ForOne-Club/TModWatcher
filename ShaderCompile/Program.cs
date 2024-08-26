namespace ShaderCompile;

public class Program
{
    public static void Main(string[] args)
    {
        foreach (var arg in args)
        {
            CompileEngine compileEngine = new(arg);
            compileEngine.Compile();
        }
        
    }
}