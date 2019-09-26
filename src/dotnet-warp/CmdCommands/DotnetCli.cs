using System.Collections.Generic;
using System.Linq;
using DotnetWarp.CmdCommands.Options;
using DotnetWarp.Extensions;

namespace DotnetWarp.CmdCommands
{
    internal class DotnetCli : CmdCommand
    {
        private readonly string _projectPath;
        private readonly bool _isVerbose;
        private readonly IEnumerable<string> _msbuildProperties;

        private static readonly Dictionary<Platform.Value, string> PlatformToRid = new Dictionary<Platform.Value, string>
        {
            [Platform.Value.Windows] = "win-x64",
            [Platform.Value.Linux] = "linux-x64",
            [Platform.Value.MacOs] = "osx-x64"
        };
        
        public DotnetCli(string projectPath, bool isVerbose, IEnumerable<string> msbuildProperties) : base(command: "dotnet")
        {
            _projectPath = projectPath;
            _isVerbose = isVerbose;
            _msbuildProperties = msbuildProperties;
        }

        public bool Publish(Context context, DotnetPublishOptions dotnetPublishOptions)
        {
            var argumentList = new List<string>
            {
                "publish",
                "-c Release",
                $"-r {dotnetPublishOptions.Rid ?? PlatformToRid[context.CurrentPlatform]}",
                $"-o {context.TempPublishPath.WithQuotes()}"
            };

            if (string.IsNullOrWhiteSpace(dotnetPublishOptions.TargetFramework))
            {
                argumentList.Add($"-f {dotnetPublishOptions.TargetFramework}");    
            }

            if (dotnetPublishOptions.IsNoRootApplicationAssemblies)
            {
                argumentList.Add("/p:RootAllApplicationAssemblies=false");    
            }
            
            if (dotnetPublishOptions.IsNoCrossGen)
            {
                argumentList.Add("/p:CrossGenDuringPublish=false");    
            }

            argumentList.AddRange(_msbuildProperties.Select(prop => $"/p:{prop}"));

            argumentList.Add($"{_projectPath.WithQuotes()}");

            var isCommandSuccessful = RunCommand(argumentList, _isVerbose);

            if (isCommandSuccessful)
            {
                context.FindAssemblyName();
            }

            return isCommandSuccessful;
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
