using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DotnetWarp.CmdCommands.Options;
using DotnetWarp.Extensions;

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
        
        private readonly bool _isVerbose;

        public WarpCli(Platform.Value platform, bool isVerbose) : base(GetWarpPath(platform))
        {
            _isVerbose = isVerbose;
        }

        public bool Pack(Context ctx, WarpPackOptions warpPackOptions)
        {
            var binFileName = ctx.CurrentPlatform == Platform.Value.Windows ? ctx.AssemblyName + ".exe" : ctx.AssemblyName;

            File.Delete(binFileName);

            var outputExePath = (warpPackOptions.OutputExePath ?? binFileName).WithQuotes();
            
            var argumentList = new List<string>
            {
                $"--arch {PlatformToWarpArch[ctx.CurrentPlatform]}",
                $"--input_dir {ctx.TempPublishPath.WithQuotes()}",
                $"--exec {binFileName.WithQuotes()}",
                $"--output {outputExePath}"
            };

            var isCommandSuccessful = RunCommand(argumentList, _isVerbose);

            if (isCommandSuccessful)
            {
                Console.WriteLine($"Saved binary to {outputExePath}");
            }

            return isCommandSuccessful;
        }
        
        private static string GetWarpPath(Platform.Value platform)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "warp", PlatformToWarpBinary[platform]);
        }
    }
}