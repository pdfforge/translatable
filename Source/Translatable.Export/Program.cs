using System;
using CommandLine;

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
                  exporter.DoExport(options);
                  return 0;
              },
                  errors => 1);
            Environment.ExitCode = exitCode;
        }
    }
}
