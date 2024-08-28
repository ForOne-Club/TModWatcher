using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace ShaderCompile;

public class FxProcessorContext : ContentProcessorContext
{
    public override ContentBuildLogger Logger { get; } = new FxBuildLogger();
    public override OpaqueDataDictionary Parameters { get; } = new();
    public override TargetPlatform TargetPlatform => TargetPlatform.Windows;
    public override GraphicsProfile TargetProfile => GraphicsProfile.HiDef;
    public override string BuildConfiguration => "Release";
    public override string OutputFilename => "output.xnb";
    public override string OutputDirectory => "XNA/Effect";
    public override string IntermediateDirectory => "XNA/Intermediate";

    public override void AddDependency(string filename)
    {
    }

    public override void AddOutputFile(string filename)
    {
    }

    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName,
        OpaqueDataDictionary processorParameters,
        string importerName, string assetName)
    {
        throw new NotImplementedException();
    }

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName,
        OpaqueDataDictionary processorParameters,
        string importerName)
    {
        throw new NotImplementedException();
    }

    public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
    {
        throw new NotImplementedException();
    }
}