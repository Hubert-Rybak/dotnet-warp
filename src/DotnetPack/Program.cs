using System;
using System.IO;
using CommandLine;
using DotnetPack.Exceptions;

namespace DotnetPack
{
    internal class Program
    {
        public class Options
        {
            [Option('p', "project", Required = false, HelpText = "Project path")]
            public DirectoryInfo ProjectPath { get; set; }
            
            [Option('r', "runtime", Required = false, HelpText = "Runtime (win-x64, linux-x64, osx-x64)")]
            public string Runtime { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool IsVerbose { get; set; }
        }


        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(options =>
            {
                try
                {
                    Validator.EnsureValid(options);

                    var projectPathName = options.ProjectPath.FullName;

                    var dotnetCli = new DotnetCli(projectPathName, options.Runtime, options.IsVerbose);
                    dotnetCli.Publish();
                }
                catch (Exception e)
                {
                    if (options.IsVerbose)
                    {
                        throw;
                    }

                    Console.WriteLine(e is DotnetPackException
                        ? $"Error: {e.Message}."
                        : $"Unhandled error: {e.Message}");

                    Environment.Exit(1);
                }
                finally
                {
                    Environment.Exit(Environment.ExitCode);
                }
            });
        }
    }
}