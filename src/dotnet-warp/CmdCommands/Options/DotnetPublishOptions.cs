namespace DotnetWarp.CmdCommands.Options
{
    internal class DotnetPublishOptions
    {
        public DotnetPublishOptions(string rid, bool isNoRootApplicationAssemblies, bool isNoCrossGen, string targetFramework)
        {
            Rid = rid;
            IsNoRootApplicationAssemblies = isNoRootApplicationAssemblies;
            IsNoCrossGen = isNoCrossGen;
            TargetFramework = targetFramework;
        }

        public string Rid { get; }
        public bool IsNoRootApplicationAssemblies { get; }
        public bool IsNoCrossGen { get; }
        public string TargetFramework { get; }
    }
}