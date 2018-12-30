using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;

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

        public ChannelReader<string> Run(IEnumerable<string> argumentList)
        {
            var processOutput = Channel.CreateUnbounded<string>();

            foreach (var argument in argumentList)
            {
                _processStartInfo.ArgumentList.Add(argument);
            }

            var process = new Process
            {
                StartInfo = _processStartInfo
            };
            
            process.OutputDataReceived += (s, e) =>
            {
                processOutput.Writer.TryWrite(e.Data);
            };
            
            process.Start();
            process.BeginOutputReadLine();

            Task.Run(() =>
            {
                process.WaitForExit();
                processOutput.Writer.Complete();
            });

            return processOutput.Reader;
        }
    }
}