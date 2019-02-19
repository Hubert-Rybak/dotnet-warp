using System;
using System.Collections.Generic;
using System.IO;

namespace DotnetWarp
{
    internal class Context : IDisposable
    {
        public string AssemblyName { get; set; }
        public string TempPublishPath { get; set; }
        public Platform.Value CurrentPlatform { get; set; }
//        public string TempFolderName { get; set; }

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
    }
}