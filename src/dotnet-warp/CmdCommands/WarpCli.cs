using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DotnetWarp.CmdCommands
{
    internal class WarpCli : CmdCommand
    {
        private static Dictionary<Platform.Value, string> _platformToWarpArch = new Dictionary<Platform.Value, string>()
        {
            [Platform.Value.Windows] = "windows-x64",
            [Platform.Value.Linux] = "linux-x64",
            [Platform.Value.MacOs] = "osx-x64"
        };
        
        private static Dictionary<Platform.Value, string> _platformToWarpBinary = new Dictionary<Platform.Value, string>()
        {
            [Platform.Value.Windows] = "windows-x64.warp-packer.exe",
            [Platform.Value.Linux] = "linux-x64.warp-packer",
            [Platform.Value.MacOs] = "macos-x64.warp-packer"
        };
        
        private readonly string _publishPath;
        private readonly bool _isVerbose;

        public WarpCli(string publishPath, Platform.Value platform, bool isVerbose) : base(GetWarpPath(platform))
        {
            _publishPath = publishPath;
            _isVerbose = isVerbose;
        }

        public bool Pack(Platform.Value platform, string projectName)
        {
            var binFileName = platform == Platform.Value.Windows ? projectName + ".exe" : projectName;

            File.Delete(binFileName);
            
            var argumentList = new ArgumentList();
            argumentList.AddArgument($"--arch {_platformToWarpArch[platform]}");
            argumentList.AddArgument($"--input_dir {_publishPath}");
            argumentList.AddArgument($"--exec {binFileName}");
            argumentList.AddArgument($"--output {binFileName}");
            
            return RunCommand(argumentList, _isVerbose);
        }
        
        private static string GetWarpPath(Platform.Value platform)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "warp", _platformToWarpBinary[platform]);
        }
    }
}