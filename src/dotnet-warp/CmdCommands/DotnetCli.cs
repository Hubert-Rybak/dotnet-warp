using System.Collections.Generic;
using DotnetWarp.CmdCommands.Options;

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
            var argumentList = new ArgumentList();
            argumentList.AddArgument("publish");
            argumentList.AddArgument($"-c Release");
            argumentList.AddArgument($"-r {dotnetPublishOptions.Rid ?? PlatformToRid[dotnetPublishOptions.Platform]}");
            argumentList.AddArgument($"-o {dotnetPublishOptions.OutputPath}");
            argumentList.AddArgument("/p:ShowLinkerSizeComparison=true");
            
            if (dotnetPublishOptions.IsNoRootApplicationAssemblies)
            {
                argumentList.AddArgument("/p:RootAllApplicationAssemblies=false");    
            }
            
            if (dotnetPublishOptions.IsNoCrossGen)
            {
                argumentList.AddArgument("/p:CrossGenDuringPublish=false");    
            }
            
            argumentList.AddArgument($"{_projectPath}");

            return RunCommand(argumentList, _isVerbose);
        }

        public bool AddLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("add");
            argumentList.AddArgument("package");
            argumentList.AddArgument("--source https://dotnet.myget.org/F/dotnet-core/api/v3/index.json ILLink.Tasks -v 0.1.5-preview-1841731");
            
            return RunCommand(argumentList, _isVerbose);
        }

        public bool RemoveLinkerPackage()
        {
            var argumentList = new ArgumentList();
            argumentList.AddArgument("remove");
            argumentList.AddArgument("package");
            argumentList.AddArgument("ILLink.Tasks");
            
            return RunCommand(argumentList, _isVerbose);
        }
    }
}