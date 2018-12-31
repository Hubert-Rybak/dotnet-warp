using System.Threading.Channels;

namespace DotnetPack.CmdCommands
{
    internal class DotnetCli : CmdCommand
    {
        private readonly string _projectPath;

        public DotnetCli(string projectPath, ChannelWriter<string> commandOutput) : base("dotnet", commandOutput)
        {
            _projectPath = projectPath;
        }

        public bool Publish(string outputPath, string rid)
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("publish");
            argumentList.AddArgument($"-c Release");
            argumentList.AddArgument($"-r {rid}");
            argumentList.AddArgument($"-o {outputPath}");
            argumentList.AddArgument($"{_projectPath}");

            return RunCommand(argumentList);
        }

        public bool AddLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("add");
            argumentList.AddArgument("package");
            argumentList.AddArgument("--source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json ILLink.Tasks -v 0.1.5-preview-1841731");
            
            return RunCommand(argumentList);
        }

        public bool RemoveLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("remove");
            argumentList.AddArgument("package");
            argumentList.AddArgument("ILLink.Tasks");
            
            return RunCommand(argumentList);
        }
    }
}