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

                actions.Add(() => dotnetCli.Publish(PublishTempPath, Runtime));
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
            foreach (var action in actions)
            {
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
                    }
                });
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
    }
}