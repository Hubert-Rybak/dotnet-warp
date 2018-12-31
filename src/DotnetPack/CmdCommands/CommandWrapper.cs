using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;

namespace DotnetPack.CmdCommands
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

        public bool Run(IEnumerable<string> argumentList, ChannelWriter<string> channelWriter)
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

            process.OutputDataReceived += ProcessOnOutputDataReceived;

            channelWriter.TryWrite($"Running {process.StartInfo.FileName} {string.Join(' ', argumentList)}");
            process.Start();

            process.BeginOutputReadLine();

            process.WaitForExit();

            process.OutputDataReceived -= ProcessOnOutputDataReceived;

            return process.ExitCode == 0;
            
            void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                channelWriter.TryWrite(args.Data);
            }
        }
    }
}