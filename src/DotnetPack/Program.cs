using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DotnetPack.CmdCommands;
using DotnetPack.Exceptions;
using Kurukuru;
using McMaster.Extensions.CommandLineUtils;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace DotnetPack
{
    [Command("dotnet-pack",
        Description = "Packs project to single binary, with optional linking.",
        OptionsComparison = StringComparison.OrdinalIgnoreCase)]
    internal class Program
    {
        [Argument(0, Description = "Project path.")]
        public string ProjectFolder { get; set; } = Directory.GetCurrentDirectory();

        [Option("-p|--platform", Description = "Optional. Sets platform for packed executable. Available values: windows|linux|macos. Defaults to current.")]
        public Platform.Value Platform { get; set; } = DotnetPack.Platform.Current();

        [Option("-l|--link-level", Description = "Optional. Sets link level. Available values: normal|aggressive.")]
        public (bool hasValue, LinkLevel value) Link { get; }

        [Option("--no-crossgen", Description = "Optional. Disables Cross Gen during publish when linker is enabled. " +
                                               "Sometimes required for linker to work. " +
                                               "See issue: https://github.com/mono/linker/issues/314")]
        public bool IsNoCrossGen { get; }

        [Option("-v|--verbose", Description = "Optional. Enables verbose output.")]
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
                var csprojsCount = Directory.EnumerateFiles(ProjectFolder, "*.csproj")
                                            .Count();

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
            var isNoRootApplicationAssemblies = Link.value == LinkLevel.Aggressive;

            try
            {
                var actions = new List<Expression<Func<bool>>>();

                if (File.Exists(ProjectFolder))
                {
                    ProjectFolder = Path.GetDirectoryName(ProjectFolder);
                }

                _tempPublishPath = Path.Combine(ProjectFolder, PublishTempPath);

                var dotnetCli = new DotnetCli(ProjectFolder, IsVerbose);
                var warp = new WarpCli(_tempPublishPath, IsVerbose);

                if (Link.hasValue)
                {
                    actions.Add(() => dotnetCli.AddLinkerPackage());
                }

                actions.Add(() => dotnetCli.Publish(PublishTempPath, Platform, isNoRootApplicationAssemblies, IsNoCrossGen));
                actions.Add(() => warp.Pack(Platform));

                if (Link.hasValue)
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
                    var hasActionSucceeded = action.Compile()
                                                   .Invoke();

                    if (hasActionSucceeded)
                    {
                        spinner.Succeed();
                    }
                    else
                    {
                        spinner.Fail();
                        errorOccured = true;
                    }
                });
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