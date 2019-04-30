namespace DotnetWarp.CmdCommands.Options
{
    internal class WarpPackOptions
    {
        public WarpPackOptions(string outputExePath)
        {
            OutputExePath = outputExePath;
        }

        public string OutputExePath { get; }
    }
}