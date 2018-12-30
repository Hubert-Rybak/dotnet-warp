using System;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using DotnetPack.Commands;
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


        static async Task Main(string[] args)
        {
            Options opt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => opt = options);
            
            try
            {
                Validator.EnsureValid(opt);

                var projectPathName = opt.ProjectPath.FullName;

                var dotnetCli = new DotnetCli(projectPathName, opt.Runtime, opt.IsVerbose);
                var outputChannel = dotnetCli.Publish();
                    
                while (await outputChannel.WaitToReadAsync())
                {
                    if (outputChannel.TryRead(out string message))
                    {
                        Console.WriteLine(message);
                    }
                }
            }
            catch (Exception e)
            {
                if (opt.IsVerbose)
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
        }
    }
}