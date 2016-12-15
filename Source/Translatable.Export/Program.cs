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
                  var resultCode = exporter.DoExport(options);

                  switch (resultCode)
                  {
                      case ResultCode.NoTranslationsFound:
                          Console.WriteLine("No translatable strings were found!");
                          break;
                      case ResultCode.NoTranslatablesFound:
                          Console.WriteLine("No classes were found that implement ITranslatable. Please check if the correct assemblied were referenced!");
                          break;
                  }

                  return (int)resultCode;
              },
                  errors => 1);
            Environment.ExitCode = exitCode;
        }
    }
}
