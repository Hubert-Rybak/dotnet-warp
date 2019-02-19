namespace DotnetWarp.CmdCommands.Options
{
    internal class WarpPackOptions
    {
        public string OutputExePath { get; }

        public WarpPackOptions(string outputExePath)
        {
            OutputExePath = outputExePath;
        }
    }
}