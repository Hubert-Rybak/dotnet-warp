using System.IO;
using System.Linq;
using System.Threading.Channels;

namespace DotnetPack.Commands
{
    internal class WarpCli : CmdCommand
    {
        private readonly string _publishPath;

        public WarpCli(string publishPath, ChannelWriter<string> commandOutput) : base(Path.Combine(Directory.GetCurrentDirectory(), "warp", "windows-x64.warp-packer.exe"), commandOutput)
        {
            _publishPath = publishPath;
        }

        public void Pack(string output)
        {
            var exeFile = Directory.EnumerateFiles(_publishPath, "*.exe").First();
            
            var argumentList = new ArgumentList();
            argumentList.AddArgument("--arch windows-x64");
            argumentList.AddArgument($"--exec {exeFile}");
            argumentList.AddArgument($"--input_dir {_publishPath}");
            argumentList.AddArgument($"--output {Path.Combine(output, Path.GetFileName(exeFile))}");
            
            RunCommand(argumentList);
        }
    }
}