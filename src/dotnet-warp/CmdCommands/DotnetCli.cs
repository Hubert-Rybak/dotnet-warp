using System.Collections.Generic;
using DotnetWarp.CmdCommands.Options;
using DotnetWarp.Extensions;

namespace DotnetWarp.CmdCommands
{
    internal class DotnetCli : CmdCommand
    {
        private readonly string _projectPath;
        private readonly bool _isVerbose;

        private static readonly Dictionary<Platform.Value, string> PlatformToRid = new Dictionary<Platform.Value, string>
        {
            [Platform.Value.Windows] = "win-x64",
            [Platform.Value.Linux] = "linux-x64",
            [Platform.Value.MacOs] = "osx-x64"
        };
        
        public DotnetCli(string projectPath, bool isVerbose) : base("dotnet")
        {
            _projectPath = projectPath;
            _isVerbose = isVerbose;
        }

        public bool Publish(DotnetPublishOptions dotnetPublishOptions)
        {
            var argumentList = new List<string>
            {
                "publish",
                "-c Release",
                $"-r {dotnetPublishOptions.Rid ?? PlatformToRid[dotnetPublishOptions.Platform]}",
                $"-o {dotnetPublishOptions.OutputPath.WithQuotes()}",
                "/p:ShowLinkerSizeComparison=true"
            };

            if (dotnetPublishOptions.IsNoRootApplicationAssemblies)
            {
                argumentList.Add("/p:RootAllApplicationAssemblies=false");    
            }
            
            if (dotnetPublishOptions.IsNoCrossGen)
            {
                argumentList.Add("/p:CrossGenDuringPublish=false");    
            }
            
            argumentList.Add($"{_projectPath.WithQuotes()}");

            return RunCommand(argumentList, _isVerbose);
        }

        public bool AddLinkerPackage()
        {
            var argumentList = new List<string>
            {
                "add",
                "package",
                "--source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json ILLink.Tasks -v 0.1.5-preview-1841731"
            };

            return RunCommand(argumentList, _isVerbose);
        }

        public bool RemoveLinkerPackage()
        {
            var argumentList = new List<string>
            {
                "remove", 
                "package", 
                "ILLink.Tasks"
            };

            return RunCommand(argumentList, _isVerbose);
        }
    }
}