using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotnetWarp
{
    internal class Context : IDisposable
    {
        public Context(string rid,
            string projectFileOrFolder,
            bool isVerbose,
            IEnumerable<string> msBuildProperties,
            LinkLevel link,
            bool isNoCrossGen,
            string targetFramework,
            string outputPath)
        {
            Rid = rid;
            ProjectFileOrFolder = projectFileOrFolder;
            IsVerbose = isVerbose;
            MsBuildProperties = msBuildProperties;
            Link = link;
            IsNoCrossGen = isNoCrossGen;
            OutputPath = outputPath;

            CurrentPlatform = rid == null ? Platform.Current() :
                rid.StartsWith("win") ? Platform.Value.Windows :
                rid.StartsWith("osx") ? Platform.Value.MacOs : Platform.Value.Linux;

            TempPublishPath = Path.Combine(projectFileOrFolder, "dotnetwarp_temp");
            TargetFramework = targetFramework;
        }

        public string AssemblyName { get; private set; }
        public string TempPublishPath { get; }
        public Platform.Value CurrentPlatform { get; }
        public string ProjectFileOrFolder { get; }
        public bool IsVerbose { get; }
        public IEnumerable<string> MsBuildProperties { get; }
        private LinkLevel Link { get; }

        public bool ShouldAddLinkerPackage => Link != LinkLevel.None;
        public bool ShouldNotRootApplicationAssemblies => Link == LinkLevel.Aggressive;
        public string Rid { get; }
        public bool IsNoCrossGen { get; }
        public string TargetFramework { get; }
        public string OutputPath { get; }

        public string OutputExeName => CurrentPlatform == Platform.Value.Windows
            ? AssemblyName + ".exe"
            : AssemblyName;

        public void Dispose()
        {
            var dirsToDelete = new List<string> {TempPublishPath, "_", "Optimize"};

            dirsToDelete.ForEach(dir =>
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            });
        }

        public void FindAssemblyName()
        {
            const string depsJsonExtension = ".deps.json";
            var depsJsonPath = Directory.EnumerateFiles(TempPublishPath, "*" + depsJsonExtension)
                                        .Single();
            var depsJsonFilename = Path.GetFileName(depsJsonPath);
            AssemblyName = depsJsonFilename.Substring(0, depsJsonFilename.Length - depsJsonExtension.Length);
        }
    }
}