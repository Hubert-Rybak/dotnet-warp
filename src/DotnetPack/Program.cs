using System;
using System.IO;
using System.Threading.Channels;
using System.Threading.Tasks;
using CommandLine;
using DotnetPack.Commands;
using DotnetPack.Exceptions;

namespace DotnetPack
{
    internal class Program
    {
        private const string PublishTempPath = "dotnetpack_temp";

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
            var commandOutputChannel = Channel.CreateUnbounded<string>();

            Options opt = null;
            Parser.Default.ParseArguments<Options>(args).WithParsed(options => opt = options);

            try
            {
                Rid.EnsureValid(opt.Runtime);
                
                var projectFolder = Path.GetDirectoryName(opt.ProjectPath.FullName);
                var publishPath = Path.Combine(projectFolder, PublishTempPath);
                
                if (opt.IsVerbose)
                {
                    Task.Run(async () => await LogToConsoleAsync(commandOutputChannel));
                }
                
                PublishProject(projectFolder, commandOutputChannel);
                PackWithWarp(publishPath, commandOutputChannel, projectFolder);
                
                Directory.Delete(publishPath, true);
                Environment.Exit(Environment.ExitCode);
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

        private static void PublishProject(string projectPathName, Channel<string> commandOutputChannel)
        {
            var dotnetCli = new DotnetCli(projectPathName, commandOutputChannel);
            dotnetCli.Publish(PublishTempPath, "Release");
        }
    }
}