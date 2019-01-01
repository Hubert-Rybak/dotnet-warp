using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using DotnetPack.CmdCommands;
using DotnetPack.Exceptions;
using Kurukuru;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace DotnetPack
{
    internal class Program
    {
        [Argument(0, Description = "Project")]
        public string ProjectFolder { get; set; } = Directory.GetCurrentDirectory();

        [Option("-r|--runtime <RID>", Description = "OPTIONAL. Runtime in RID format. Example = win-x64. If not set, default to current RID.")]
        public string Runtime { get; set; } = Rid.Current();

        [Option("-l|--link", Description = "OPTIONAL. Enables linker.")]
        public bool IsLinkerEnabled { get; }
        
        [Option("--no-root", Description = "LINKER OPTION. Sets RootAllApplicationAssemblies to false, allows for more aggressive linking")]
        public bool IsNoRootApplicationAssemblies { get; }
        
        [Option("--no-crossgen", Description = "LINKER OPTION. Sets CrossGenDuringPublish to false, disables Cross Gen during publish. See issue: https://github.com/mono/linker/issues/314")]
        public bool IsNoCrossGen { get; }

        [Option("-v|--verbose", Description = "Enable verbose output.")]
        public bool IsVerbose { get; }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string _tempPublishPath;
        private const string PublishTempPath = "dotnetpack_temp";

        private ValidationResult OnValidate()
        {
            if (IsNoRootApplicationAssemblies || IsNoCrossGen )
            {
                if (IsLinkerEnabled == false)
                {
                    return new ValidationResult("Specified linker option without --linker flag.");
                }
            } 
            
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
                var csprojsCount = Directory.EnumerateFiles(ProjectFolder, "*.csproj").Count();
                
                if (csprojsCount == 0)
                {
                    return new ValidationResult($"No .csproj file found.");
                }
                
                if (csprojsCount > 1)
                {
                    return new ValidationResult("More than one .csproj file found. Specify single with --project flag.");
                }
                
                if (csprojsCount == 1)
                {
                    return ValidationResult.Success;
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
                var actions = new List<Expression<Func<bool>>>();

                if (File.Exists(ProjectFolder))
                {
                    ProjectFolder = Path.GetDirectoryName(ProjectFolder);
                }

                _tempPublishPath = Path.Combine(ProjectFolder, PublishTempPath);

                if (IsVerbose)
                {
                    Task.Run(async () => await LogToConsoleAsync(commandOutputChannel));
                }

                var dotnetCli = new DotnetCli(ProjectFolder, commandOutputChannel);
                var warp = new WarpCli(_tempPublishPath, commandOutputChannel);

                if (IsLinkerEnabled)
                {
                    actions.Add(() => dotnetCli.AddLinkerPackage());
                }

                actions.Add(() => dotnetCli.Publish(PublishTempPath, Runtime, IsNoRootApplicationAssemblies, IsNoCrossGen));
                actions.Add(() => warp.Pack(ProjectFolder));
                
                if (IsLinkerEnabled)
                {
                    actions.Add(() => dotnetCli.RemoveLinkerPackage());
                }

                RunActions(actions);
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

        private void RunActions(List<Expression<Func<bool>>> actions)
        {
            bool errorOccured = false;
            foreach (var action in actions)
            {
                if (errorOccured)
                {
                    Console.WriteLine("Error occured. Set --verbose flag for more info.");
                    return;
                }
                Spinner.Start("Packing...", spinner =>
                {
                    spinner.Text = $"Running {((MethodCallExpression) action.Body).Method.Name}...";
                    var hasActionSucceeded = action.Compile().Invoke();
                    
                    if (hasActionSucceeded)
                    {
                        spinner.Succeed();
                    }
                    else
                    {
                        spinner.Fail();
                        errorOccured = true;
                    }
                }, Patterns.CircleHalves);
            }
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

        private static void DeleteTempFolders()
        {
            var dirsToDelete = new List<string>() {_tempPublishPath, "_", "Optimize"};
            
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