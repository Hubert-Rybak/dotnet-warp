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
        private static string _tempPublishPath;
        private const string PublishTempPath = "dotnetpack_temp";

        public class Options
        {
            [Option('p', "project", Required = false, HelpText = "Project path")]
            public DirectoryInfo ProjectPath { get; set; }

            [Option('r', "runtime", Required = false, HelpText = "Runtime")]
            public string Runtime { get; set; }

            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool IsVerbose { get; set; }
        }

        static void Main(string[] args)
        {
            var commandOutputChannel = Channel.CreateUnbounded<string>();

            Options opt = null;
            
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => opt = options)
                .WithNotParsed(errors => Environment.Exit(1));
            try
            {
                if (opt.Runtime == null)
                {
                    opt.Runtime = Rid.Current();
                    Console.WriteLine($"No runtime specified, selected current ({opt.Runtime}).");
                }

                var projectPath = opt.ProjectPath != null
                    ? Path.GetDirectoryName(opt.ProjectPath.FullName)
                    : Directory.GetCurrentDirectory();

                _tempPublishPath = Path.Combine(projectPath, PublishTempPath);

                if (opt.IsVerbose)
                {
                    Task.Run(async () => await LogToConsoleAsync(commandOutputChannel));

                    Console.WriteLine($"Project path: {projectPath}");
                    Console.WriteLine($"Publish path: {_tempPublishPath}");
                }

                PublishProject(projectPath, opt.Runtime, commandOutputChannel);
                PackWithWarp(_tempPublishPath, commandOutputChannel, projectPath);
            }
            catch (Exception e)
            {
                Environment.ExitCode = 1;
                if (opt.IsVerbose)
                {
                    throw;
                }

                Console.WriteLine(e is DotnetPackException
                    ? $"Error: {e.Message}."
                    : $"Unhandled error: {e.Message}");
            }
            finally
            {
                if (Directory.Exists(_tempPublishPath))
                {
                    Directory.Delete(_tempPublishPath, true);
                }

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

        private static void PublishProject(string projectPathName, string rid, Channel<string> commandOutputChannel)
        {
            var dotnetCli = new DotnetCli(projectPathName, commandOutputChannel);
            dotnetCli.Publish(PublishTempPath, "Release", rid);
        }
    }
}