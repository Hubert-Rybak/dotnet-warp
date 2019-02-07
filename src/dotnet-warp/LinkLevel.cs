namespace DotnetWarp
{
    /// <summary>
    /// Sets linking level for linking with ILLink.Tasks
    /// </summary>
    internal enum LinkLevel
    {
        None = 0,
        // Run with default options
        Normal = 1,
        // Also links application assemblies (RootAllApplicationAssemblies=false)
        Aggressive = 2
    }
}