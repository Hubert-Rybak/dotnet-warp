using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DotnetWarp.CmdCommands.Options;

namespace DotnetWarp.CmdCommands
{
    internal class WarpCli : CmdCommand
    {
        private static readonly Dictionary<Platform.Value, string> PlatformToWarpArch = new Dictionary<Platform.Value, string>
        {
            [Platform.Value.Windows] = "windows-x64",
            [Platform.Value.Linux] = "linux-x64",
            [Platform.Value.MacOs] = "macos-x64"
        };
        
        private static readonly Dictionary<Platform.Value, string> PlatformToWarpBinary = new Dictionary<Platform.Value, string>
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

        public bool Pack(WarpPackOptions warpPackOptions)
        {
            var binFileName = warpPackOptions.Platform == Platform.Value.Windows ? warpPackOptions.ProjectName + ".exe" : warpPackOptions.ProjectName;

            File.Delete(binFileName);
            
            var argumentList = new ArgumentList();
            argumentList.AddArgument($"--arch {PlatformToWarpArch[warpPackOptions.Platform]}");
            argumentList.AddArgument($"--input_dir {_publishPath}");
            argumentList.AddArgument($"--exec {binFileName}");
            argumentList.AddArgument($"--output {binFileName}");
            
            return RunCommand(argumentList, _isVerbose);
        }
        
        private static string GetWarpPath(Platform.Value platform)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "warp", PlatformToWarpBinary[platform]);
        }
    }
}