namespace DotnetWarp.CmdCommands.Options
{
    internal class DotnetPublishOptions
    {
        public DotnetPublishOptions(string outputPath, string rid, Platform.Value platform, bool isNoRootApplicationAssemblies, bool isNoCrossGen)
        {
            OutputPath = outputPath;
            Rid = rid;
            Platform = platform;
            IsNoRootApplicationAssemblies = isNoRootApplicationAssemblies;
            IsNoCrossGen = isNoCrossGen;
        }

        public string OutputPath { get; }
        public string Rid { get; }
        public Platform.Value Platform { get; }
        public bool IsNoRootApplicationAssemblies { get; }
        public bool IsNoCrossGen { get; }
    }
}