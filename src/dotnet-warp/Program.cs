using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DotnetWarp.CmdCommands;
using DotnetWarp.CmdCommands.Options;
using DotnetWarp.Exceptions;
using McMaster.Extensions.CommandLineUtils;
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace DotnetWarp
{
    [Command(Description = "Packs project to single binary, with optional linking.", OptionsComparison = StringComparison.OrdinalIgnoreCase)]
    internal class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);
        
        [Argument(0, Description = "Project path.")]
        public string ProjectFileOrFolder { get; set; } = Directory.GetCurrentDirectory();

        [Option("-r|--rid <RID>", Description = "Optional. Sets RID passed to dotnet publish. Defaults to current portable RID (win-x64, linux-x64, osx-x64).")]
        public string Rid { get; }
        
        [Option("-l|--link-level <LEVEL>", Description = "Optional. Enables linking with desired level. Available values: Normal, Aggressive. " +
                                                         "Aggressive means, that application assemblies will not be rooted, and can also be trimmed.")]
        public LinkLevel Link { get; }

        [Option("-nc|--no-crossgen", Description = "Optional linker option. Disables Cross Gen during publish. " +
                                                   "Sometimes required for linker to work. " +
                                                   "See issue: https://github.com/mono/linker/issues/314")]
        public bool IsNoCrossGen { get; }

        [Option("-o|--output", Description = "Optional. Output exe path. Defaults to current directory + assembly name.")]
        public string Output { get; }
        
        [Option("-v|--verbose", Description = "Optional. Enables verbose output.")]
        public bool IsVerbose { get; }

        private Context BuildContext()
        {
            var context = new Context();

            context.CurrentPlatform = Rid == null ? Platform.Current() :
                Rid.StartsWith("win") ? Platform.Value.Windows :
                Rid.StartsWith("osx") ? Platform.Value.MacOs : Platform.Value.Linux;

            context.TempPublishPath = Path.Combine(ProjectFileOrFolder, "dotnetwarp_temp");

            return context;
        }

        private ValidationResult OnValidate()
        {
            if (string.IsNullOrEmpty(Path.GetFileName(Output)))
            {
                return new ValidationResult("Output should contain full path to file.");
            }
            
            if (File.Exists(ProjectFileOrFolder))
            {
                if (!string.Equals(Path.GetExtension(ProjectFileOrFolder), ".csproj", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(Path.GetExtension(ProjectFileOrFolder), ".fsproj", StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidationResult("Specified file is not .csproj or .fsproj file.");
                }

                ProjectFileOrFolder = Path.GetDirectoryName(ProjectFileOrFolder);
                
                return ValidationResult.Success;
            }

            if (Directory.Exists(ProjectFileOrFolder))
            {
                var projsCount = 
                    Directory.EnumerateFiles(ProjectFileOrFolder, "*.csproj").Count() +
                    Directory.EnumerateFiles(ProjectFileOrFolder, "*.fsproj").Count();

                if (projsCount == 0)
                {
                    return new ValidationResult($"No .csproj or .fsproj file found.");
                }

                if (projsCount > 1)
                {
                    return new ValidationResult("More than one .*csproj or .fsproj file found. Specify single with --project flag.");
                }
            }

            return ValidationResult.Success;
        }

        private void OnExecute()
        {
            if (File.Exists(ProjectFileOrFolder))
            {
                ProjectFileOrFolder = Path.GetDirectoryName(ProjectFileOrFolder);
            }
            
            Context context = BuildContext();

            try
            {
                var actions = new List<Expression<Func<Context, bool>>>();
                
                var dotnetCli = new DotnetCli(ProjectFileOrFolder, IsVerbose);
                var warp = new WarpCli(context.CurrentPlatform, IsVerbose);

                if (Link != LinkLevel.None)
                {
                    actions.Add((ctx) => dotnetCli.AddLinkerPackage());
                }
                
                var isNoRootApplicationAssemblies = Link == LinkLevel.Aggressive;

                actions.Add((ctx) => dotnetCli.Publish(ctx, new DotnetPublishOptions(Rid, isNoRootApplicationAssemblies, IsNoCrossGen)));

                actions.Add((ctx) => warp.Pack(ctx, new WarpPackOptions(Output)));

                if (Link != LinkLevel.None)
                {
                    actions.Add((ctx) => dotnetCli.RemoveLinkerPackage());
                }

                RunActions(actions, context);
            }
            catch (Exception e)
            {
                Environment.ExitCode = 1;
                if (IsVerbose)
                {
                    throw;
                }

                Console.WriteLine(e is DotnetWarpException
                                      ? $"Error: {e.Message}."
                                      : $"Unhandled error: {e.Message}");
            }
            finally
            {
                context.Dispose();
            }
        }

        private void RunActions(List<Expression<Func<Context, bool>>> actions, Context ctx)
        {
            foreach (var action in actions)
            {
                var actionName = ((MethodCallExpression) action.Body).Method.Name;
                
                Console.WriteLine($"Running {actionName}...");
                var hasActionSucceeded = action.Compile().Invoke(ctx);

                if (!hasActionSucceeded)
                {
                    Console.WriteLine($"{actionName} failed. Set --verbose flag for more info.");
                }
            }
        }
    }
}