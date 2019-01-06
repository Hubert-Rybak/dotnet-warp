using System.Collections.Generic;

namespace DotnetPack.CmdCommands
{
    internal class CmdCommand
    {
        private readonly CommandWrapper _commandWrapper;

        protected CmdCommand(string command)
        {
            _commandWrapper = new CommandWrapper(command);
        }
        
        protected bool RunCommand(IEnumerable<string> arguments, bool isVerbose)
        {
            return _commandWrapper.Run(arguments, isVerbose);
        }
    }
}