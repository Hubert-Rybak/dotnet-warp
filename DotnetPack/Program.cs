using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace DotnetPack
{
    class Program
    {
        static int Main(string[] args)
        {
            // Create some options and a parser
            Option optionThatTakesInt = new Option(
                "--project",
                "Project path",
                new Argument<string>());
            Option optionThatTakesBool = new Option(
                "--output",
                "An option whose argument is parsed as a bool",
                new Argument<bool>());
            Option optionThatTakesFileInfo = new Option(
                "--file-option",
                "An option whose argument is parsed as a FileInfo",
                new Argument<FileInfo>());

            // Add them to the root command
            var rootCommand = new RootCommand();
            rootCommand.Description = "My sample app";
            rootCommand.AddOption(optionThatTakesInt);
            rootCommand.AddOption(optionThatTakesBool);
            rootCommand.AddOption(optionThatTakesFileInfo);

            rootCommand.Handler = CommandHandler.Create<string, bool, FileInfo>((intOption, boolOption, fileOption) =>
            {
                Console.WriteLine($"The value for --int-option is: {intOption}");
                Console.WriteLine($"The value for --bool-option is: {boolOption}");
                Console.WriteLine($"The value for --file-option is: {fileOption?.FullName ?? "null"}");
            });

            // Parse the incoming args and invoke the handler
            return rootCommand.InvokeAsync(args).Result;
        }    }
}