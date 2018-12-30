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

        public void Publish(string outputPath, string configuration, string rid = null)
        {
            rid = rid ?? Rid.CurrentRid();
            configuration = configuration ?? "Release";

            var argumentList = new ArgumentList();
            argumentList.AddArgument("publish");
            argumentList.AddArgument($"-c {configuration}");
            argumentList.AddArgument($"-r {rid}");
            argumentList.AddArgument($"-o {outputPath}");
            argumentList.AddArgument($"{_projectPath}");

            RunCommand(argumentList);
        }
    }
}