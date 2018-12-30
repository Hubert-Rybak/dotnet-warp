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

        public ChannelReader<string> Run(string arguments)
        {
            var processOutput = Channel.CreateUnbounded<string>();

            _processStartInfo.Arguments = arguments;
            
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