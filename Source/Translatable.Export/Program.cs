using System;
using System.Diagnostics;
using CommandLine;
using Translatable.Export.Po;

namespace Translatable.Export
{
    class Program
    {
        static void Main(string[] args)
        {
            var exporter = new Exporter();

            var result = CommandLine.Parser.Default.ParseArguments<ExportOptions>(args);
            var exitCode = result
              .MapResult(options =>
                    {
                        return exporter.DoExport(options.Assemblies).Match(
                            some: catalog =>
                            {
                                var writer = new PotWriter();
                                writer.WritePotFile(options.OutputFile, catalog);
                                return 0;
                            },
                            none: resultCode =>
                            {
                                switch (resultCode)
                                {
                                    case ResultCode.NoTranslationsFound:
                                        Console.WriteLine("No translatable strings were found!");
                                        break;
                                    case ResultCode.NoTranslatablesFound:
                                        Console.WriteLine(
                                            "No classes were found that implement ITranslatable. Please check if the correct assemblies were referenced!");
                                        break;
                                }

                                return (int) resultCode;
                            });
                    },
                  errors => 1);
            Environment.ExitCode = exitCode;

            if (Debugger.IsAttached)
                Console.Read();
        }
    }
}
