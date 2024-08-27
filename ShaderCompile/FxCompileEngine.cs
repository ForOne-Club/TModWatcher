using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;

namespace ShaderCompile;

public class FxCompileEngine(string fxFilePath)
{
    public void Compile()
    {
        // 实例化上下文
        FxProcessorContext fxProcessor = new();
        ContentImporter<EffectContent> effectImporter = new EffectImporter();
        var processor = new EffectProcessor();

        // 读取文件
        FxImporterContext fxImporterContext = new();
        EffectContent effectContent = effectImporter.Import(fxFilePath, fxImporterContext);

        // 编译文件
        CompiledEffectContent compiledEffectContent = processor.Process(effectContent, fxProcessor);

        Type contentCompilerType =
            typeof(ContentCompiler).Assembly.GetType("Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler.ContentCompiler");

        if (contentCompilerType == null) return;
        ConstructorInfo contentCompilerCtor = contentCompilerType
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First();

        var contentCompiler = (ContentCompiler)contentCompilerCtor.Invoke(null);

        MethodInfo compileMethodInfo = contentCompilerType.GetMethod("Compile", BindingFlags.Instance | BindingFlags.NonPublic);
        if (compileMethodInfo == null) return;
        if (Path.GetDirectoryName(fxFilePath) is not { } directoryPath) return;
        using FileStream fileStream = new($"{Path.Combine(directoryPath, Path.GetFileNameWithoutExtension(fxFilePath))}.xnb", FileMode.Create);
        compileMethodInfo.Invoke(contentCompiler, [
            fileStream,
            compiledEffectContent,
            TargetPlatform.Windows,
            GraphicsProfile.HiDef,
            false,
            AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
            AppDomain.CurrentDomain.BaseDirectory
                .TrimEnd('\\') //this param is called referenceRelocationPath. not sure exactly what it does
        ]);
    }
}