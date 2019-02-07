using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DotnetWarp.CmdCommands
{
    public class CommandWrapper
    {
        private readonly ProcessStartInfo _processStartInfo;

        public CommandWrapper(string commandName)
        {
            _processStartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                FileName = commandName
            };
        }

        public bool Run(IEnumerable<string> argumentList, bool isVerbose)
        {
            _processStartInfo.ArgumentList.Clear();
            
            foreach (var argument in argumentList)
            {
                _processStartInfo.ArgumentList.Add(argument);
            }

            var process = new Process
            {
                StartInfo = _processStartInfo
            };

            if (isVerbose)
            {
                process.OutputDataReceived += ProcessOnOutputDataReceived;
                Console.WriteLine($"Running {process.StartInfo.FileName} {string.Join(' ', argumentList)}");
            }
            
            process.Start();

            process.BeginOutputReadLine();

            process.WaitForExit();

            if (isVerbose)
            {
                process.OutputDataReceived -= ProcessOnOutputDataReceived;
            }

            return process.ExitCode == 0;
            
            void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                Console.WriteLine(args.Data);
            }
        }
    }
}