using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DotnetPack
{
    internal enum Command
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
        private ProcessStartInfo _defaultStartInfo;

        private Dictionary<Command, string> CommandsMap => new Dictionary<Command, string>()
        {
            [Command.Publish] = $"publish -c Release -r {_rid} {_projectPathName}"
        };
        
        public DotnetCli(string projectPathName, string rid, bool isVerbose)
        {
            _projectPathName = projectPathName;
            _rid = rid ?? Rid.CurrentRid();
            _isVerbose = isVerbose;
            _defaultStartInfo = new ProcessStartInfo()
            {
                RedirectStandardError = isVerbose,
                RedirectStandardOutput = isVerbose,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = DotnetCliName
            };
        }


        public void Publish()
        {
            RunCommand(Command.Publish);
        }

        private void RunCommand(Command arguments)
        {
            _defaultStartInfo.Arguments = CommandsMap[arguments];
            var process = new Process
            {
                StartInfo = _defaultStartInfo
            };
            
            if (_isVerbose)
            {
                process.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
            }

            process.WaitForExit();
        }
    }
}