using System.IO;
using System.Linq;

namespace DotnetWarp.CmdCommands.Options
{
    internal class WarpPackOptions
    {
        public WarpPackOptions(Platform.Value platform, string projectFolder)
        {
            Platform = platform;
            ProjectName = GetProjectName(projectFolder);
        }

        public Platform.Value Platform { get; }
        public string ProjectName { get; }

        private string GetProjectName(string projectFolder)
        {
            var projectFile = Directory.EnumerateFiles(projectFolder, "*.csproj")
                                       .Single();

            return Path.GetFileNameWithoutExtension(projectFile);
        }
    }
}