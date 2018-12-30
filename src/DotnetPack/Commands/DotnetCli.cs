using System;
using System.Threading.Channels;

namespace DotnetPack.Commands
{
    internal class DotnetCli : CmdCommand
    {
        private readonly string _projectPath;

        public DotnetCli(string projectPath, ChannelWriter<string> commandOutput) : base("dotnet", commandOutput)
        {
            _projectPath = projectPath;
        }

        public void Publish(string outputPath, string configuration, string rid)
        {
            configuration = configuration ?? "Release";

            var argumentList = new ArgumentList();
            argumentList.AddArgument("publish");
            argumentList.AddArgument($"-c {configuration}");
            argumentList.AddArgument($"-r {rid}");
            argumentList.AddArgument($"-o {outputPath}");
            argumentList.AddArgument($"{_projectPath}");

            RunCommand(argumentList);
        }

        public void AddLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("add");
            argumentList.AddArgument("package");
            argumentList.AddArgument("--source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json ILLink.Tasks -v 0.1.5-preview-1841731");
            
            RunCommand(argumentList);
        }

        public void RemoveLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("remove");
            argumentList.AddArgument("package");
            argumentList.AddArgument("ILLink.Tasks");
            
            RunCommand(argumentList);
        }
    }
}