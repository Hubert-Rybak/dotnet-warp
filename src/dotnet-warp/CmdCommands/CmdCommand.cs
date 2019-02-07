using System.Collections.Generic;

namespace DotnetWarp.CmdCommands
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