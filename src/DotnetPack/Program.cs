using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotnetPack.Commands;
using DotnetPack.Exceptions;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace DotnetPack
{
    internal class Program
    {
        [Argument(0, Description = "Project")]
        public string ProjectFolder { get; set; } = Directory.GetCurrentDirectory();

        [Option("-r|--runtime <RID>", Description = "Runtime")]
        public string Runtime { get; set; } = Rid.Current();

        [Option("-l|--linker", Description = "Enable linker")]
        public bool IsLinkerEnabled { get; }

        [Option("-v|--verbose", Description = "Set output to verbose messages.")]
        public bool IsVerbose { get; }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string _tempPublishPath;
        private const string PublishTempPath = "dotnetpack_temp";

        private ValidationResult OnValidate()
        {
            if (File.Exists(ProjectFolder))
            {
                if (!string.Equals(Path.GetExtension(ProjectFolder), "csproj", StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult("Specified file is not .csproj file.");
                }

                ProjectFolder = Path.GetDirectoryName(ProjectFolder);
                return ValidationResult.Success;

            }

            if (Directory.Exists(ProjectFolder))
            {
                var csprojs = Directory.EnumerateFiles(ProjectFolder, "*.csproj");
                if (csprojs.Count() > 1)
                {
                    return new ValidationResult("More than one .csproj file found. Specify single with --project flag.");
                }
                return ValidationResult.Success;
            }
            
            return new ValidationResult("Not valid project path specified.");
        }

        private void OnExecute()
        {
            var commandOutputChannel = Channel.CreateUnbounded<string>();

            try
            {
                if (File.Exists(ProjectFolder))
                {
                    ProjectFolder = Path.GetDirectoryName(ProjectFolder);
                }

                _tempPublishPath = Path.Combine(ProjectFolder, PublishTempPath);

                if (IsVerbose)
                {
                    Task.Run(async () => await LogToConsoleAsync(commandOutputChannel));

                    Console.WriteLine($"Project path: {ProjectFolder}");
                    Console.WriteLine($"Publish path: {_tempPublishPath}");
                }

                var dotnetCli = new DotnetCli(ProjectFolder, commandOutputChannel);

                if (IsLinkerEnabled)
                {
                    dotnetCli.AddLinkerPackage();
                }

                dotnetCli.Publish(PublishTempPath, "Release", Runtime);

                PackWithWarp(_tempPublishPath, commandOutputChannel, ProjectFolder);

                if (IsLinkerEnabled)
                {
                    dotnetCli.RemoveLinkerPackage();
                }
            }
            catch (Exception e)
            {
                Environment.ExitCode = 1;
                if (IsVerbose)
                {
                    throw;
                }

                Console.WriteLine(e is DotnetPackException
                                      ? $"Error: {e.Message}."
                                      : $"Unhandled error: {e.Message}");
            }
            finally
            {
                DeleteTempFolders();
            }
        }

        private static void DeleteTempFolders()
        {
            if (Directory.Exists(_tempPublishPath))
            {
                Directory.Delete(_tempPublishPath, true);
            }

            if (Directory.Exists("_"))
            {
                Directory.Delete("_", true);
            }

            if (Directory.Exists("Optimize"))
            {
                Directory.Delete("Optimize", true);
            }
        }

        private static void PackWithWarp(string publishPath, Channel<string> commandOutputChannel, string projectFolder)
        {
            var warp = new WarpCli(publishPath, commandOutputChannel);
            warp.Pack(projectFolder);
        }

        private static async Task LogToConsoleAsync(Channel<string> commandOutputChannel)
        {
            while (await commandOutputChannel.Reader.WaitToReadAsync())
            {
                if (commandOutputChannel.Reader.TryRead(out string message))
                {
                    Console.WriteLine(message);
                }
            }
        }
    }
}