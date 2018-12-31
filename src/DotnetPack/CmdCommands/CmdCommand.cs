using System.Collections.Generic;
using System.Threading.Channels;

namespace DotnetPack.CmdCommands
{
    internal class CmdCommand
    {
        private readonly ChannelWriter<string> _commandOutput;
        private readonly CommandWrapper _commandWrapper;

        public CmdCommand(string command, ChannelWriter<string> commandOutput)
        {
            _commandOutput = commandOutput;
            _commandWrapper = new CommandWrapper(command);
        }
        
        protected bool RunCommand(IEnumerable<string> arguments)
        {
            return _commandWrapper.Run(arguments, _commandOutput);
        }
    }
}