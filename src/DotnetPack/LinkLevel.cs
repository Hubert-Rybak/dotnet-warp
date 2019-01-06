namespace DotnetPack
{
    /// <summary>
    /// Sets linking level for linking with ILLink.Tasks
    /// </summary>
    internal enum LinkLevel
    {
        // Run with default options
        Normal = 1,
        // Also links application assemblies (RootAllApplicationAssemblies=false)
        Aggressive = 2
    }
}