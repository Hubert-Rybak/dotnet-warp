using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using DotnetWarp.CmdCommands.Options;
using DotnetWarp.Extensions;

namespace DotnetWarp.CmdCommands
{
    internal class WarpCli : CmdCommand
    {
        private static readonly Dictionary<Platform.Value, string> PlatformToWarpArch =
            new Dictionary<Platform.Value, string>
            {
                [Platform.Value.Windows] = "windows-x64",
                [Platform.Value.Linux] = "linux-x64",
                [Platform.Value.MacOs] = "macos-x64"
            };

        private readonly bool _isVerbose;

        public WarpCli(bool isVerbose) : base(GetWarpPath())
        {
            _isVerbose = isVerbose;
        }

        public bool Pack(Context ctx, WarpPackOptions warpPackOptions)
        {
            File.Delete(ctx.OutputExeName);

            var outputExePath = (warpPackOptions.OutputExePath ?? ctx.OutputExeName).WithQuotes();

            var argumentList = new List<string>
            {
                $"--arch {PlatformToWarpArch[ctx.CurrentPlatform]}",
                $"--input_dir {ctx.TempPublishPath.WithQuotes()}",
                $"--exec {ctx.OutputExeName.WithQuotes()}",
                $"--output {outputExePath}"
            };

            var isCommandSuccessful = RunCommand(argumentList, _isVerbose);

            if (isCommandSuccessful)
            {
                Console.WriteLine($"Saved binary to {outputExePath}");
            }

            return isCommandSuccessful;
        }

        private static string GetWarpPath()
        {
            string warpPacker;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                warpPacker = "linux-x64.warp-packer";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                warpPacker = "macos-x64.warp-packer";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                warpPacker = "windows-x64.warp-packer.exe";
            else
                throw new PlatformNotSupportedException();
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                              .Location), "warp", warpPacker);
        }
    }
}