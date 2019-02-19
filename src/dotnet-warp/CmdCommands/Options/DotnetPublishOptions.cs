namespace DotnetWarp.CmdCommands.Options
{
    internal class DotnetPublishOptions
    {
        public DotnetPublishOptions(string rid, bool isNoRootApplicationAssemblies, bool isNoCrossGen)
        {
            Rid = rid;
            IsNoRootApplicationAssemblies = isNoRootApplicationAssemblies;
            IsNoCrossGen = isNoCrossGen;
        }

        public string Rid { get; }
        public bool IsNoRootApplicationAssemblies { get; }
        public bool IsNoCrossGen { get; }
    }
}