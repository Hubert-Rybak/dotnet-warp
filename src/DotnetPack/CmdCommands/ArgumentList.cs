using System;
using System.Collections;
using System.Collections.Generic;

namespace DotnetPack.CmdCommands
{
    public class ArgumentList : IEnumerable<string>
    {
        private const char Space = ' ';
        private readonly List<string> _arguments = new List<string>();

        public void AddArgument(string argument)
        {
            foreach (var arg in argument.Split(Space, StringSplitOptions.RemoveEmptyEntries))
            {
                _arguments.Add(arg);
            }
        }
        
        public IEnumerator<string> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}