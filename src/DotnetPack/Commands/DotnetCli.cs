using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DotnetPack.Commands
{
    internal enum DotnetCommand
    {
        Publish,
        AddLinker
    }
    
    internal class DotnetCli
    {
        private const string DotnetCliName = "dotnet";
        
        private readonly string _projectPathName;
        private readonly string _rid;
        private readonly bool _isVerbose;
        private CommandWrapper _dotnetCommandWrapper;

        private Dictionary<DotnetCommand, string> CommandToArgumentMap => new Dictionary<DotnetCommand, string>()
        {
            [DotnetCommand.Publish] = $"publish -c Release -r {_rid} -o dotpack_temp {_projectPathName}"
        };
        
        public DotnetCli(string projectPathName, string rid, bool isVerbose)
        {
            _projectPathName = projectPathName;
            _rid = rid ?? Rid.CurrentRid();
            _isVerbose = isVerbose;
            _dotnetCommandWrapper = new CommandWrapper("dotnet");
        }

        public ChannelReader<string> Publish()
        {
            return RunCommand(DotnetCommand.Publish);
        }

        private ChannelReader<string> RunCommand(DotnetCommand dotnetCommand)
        {
            var arguments = CommandToArgumentMap[dotnetCommand];

            return _dotnetCommandWrapper.Run(arguments);
        }
    }
}