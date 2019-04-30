using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

        public int Run(IEnumerable<string> argumentList, bool isVerbose)
        {
            var arguments = string.Join(' ', argumentList);

            _processStartInfo.Arguments = arguments;

            var process = new Process
            {
                StartInfo = _processStartInfo,
                EnableRaisingEvents = true
            };

            if (isVerbose)
            {
                process.OutputDataReceived += ProcessOnOutputDataReceived;
                Console.WriteLine($"Running {process.StartInfo.FileName} {arguments}");
            }

            process.Start();
            process.BeginOutputReadLine();

            if (process.HasExited)
            {
                Task.FromResult(process.ExitCode);
            }

            var tcs = new TaskCompletionSource<int>();
            process.Exited += (sender, args) =>
            {
                process.OutputDataReceived -= ProcessOnOutputDataReceived;
                tcs.SetResult(process.ExitCode);
            };

            return tcs.Task.Result;

            void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                Console.WriteLine(args.Data);
            }
        }
    }
}