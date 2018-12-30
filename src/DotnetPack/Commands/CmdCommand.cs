using System.Collections.Generic;
using System.Threading.Channels;

namespace DotnetPack.Commands
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
        
        protected void RunCommand(IEnumerable<string> arguments)
        {
            _commandWrapper.Run(arguments, _commandOutput);
        }
    }
}