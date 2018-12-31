using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Channels;

namespace DotnetPack.CmdCommands
{
    internal class WarpCli : CmdCommand
    {
        private readonly string _publishPath;

        public WarpCli(string publishPath, ChannelWriter<string> commandOutput) : base(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "warp", "windows-x64.warp-packer.exe"), commandOutput)
        {
            _publishPath = publishPath;
        }

        public bool Pack(string output)
        {
            var exeFile = Directory.EnumerateFiles(_publishPath, "*.exe").First();
            var fileName = Path.GetFileName(exeFile);

            File.Delete(fileName);
            
            var argumentList = new ArgumentList();
            argumentList.AddArgument("--arch windows-x64");
            argumentList.AddArgument($"--input_dir {_publishPath}");
            argumentList.AddArgument($"--exec {fileName}");
            argumentList.AddArgument($"--output {fileName}");
            
            return RunCommand(argumentList);
        }
    }
}