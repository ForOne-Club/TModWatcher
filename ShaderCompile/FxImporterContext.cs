using Microsoft.Xna.Framework.Content.Pipeline;

namespace ShaderCompile;

public class FxImporterContext : ContentImporterContext
{
    public override string OutputDirectory => "XNA/Effect";
    public override string IntermediateDirectory => "XNA/Intermediate";

    public override ContentBuildLogger Logger => new FxBuildLogger();

    public override void AddDependency(string filename)
    {
    }
}