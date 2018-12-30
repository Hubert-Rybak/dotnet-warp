using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Channels;

namespace DotnetPack.Commands
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

        public void Run(IEnumerable<string> argumentList, ChannelWriter<string> channelWriter)
        {
            foreach (var argument in argumentList)
            {
                _processStartInfo.ArgumentList.Add(argument);
            }

            var process = new Process
            {
                StartInfo = _processStartInfo
            };

            process.OutputDataReceived += ProcessOnOutputDataReceived;

            process.Start();

            process.BeginOutputReadLine();

            process.WaitForExit();

            process.OutputDataReceived -= ProcessOnOutputDataReceived;

            void ProcessOnOutputDataReceived(object sender, DataReceivedEventArgs args)
            {
                channelWriter.TryWrite(args.Data);
            }
        }
    }
}