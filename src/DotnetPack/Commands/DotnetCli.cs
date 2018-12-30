using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotnetPack.Commands
{
    internal class DotnetCli
    {
        private readonly string _projectPathName;
        private const string DotnetCliName = "dotnet";
        private readonly CommandWrapper _dotnetCommandWrapper;

        public DotnetCli(string projectPathName)
        {
            _projectPathName = projectPathName;
            _dotnetCommandWrapper = new CommandWrapper(DotnetCliName);
        }

        public ChannelReader<string> Publish(string outputPath, string configuration, string rid = null)
        {
            rid = rid ?? Rid.CurrentRid();
            configuration = configuration ?? "Release";

            var argumentList = new ArgumentList();
            argumentList.AddArgument("publish");
            argumentList.AddArgument($"-c {configuration}");
            argumentList.AddArgument($"-r {rid}");
            argumentList.AddArgument($"-o {outputPath}");
            argumentList.AddArgument($"{_projectPathName}");

            return RunCommand(argumentList);
        }

        private ChannelReader<string> RunCommand(IEnumerable<string> arguments)
        {
            return _dotnetCommandWrapper.Run(arguments);
        }
    }
}