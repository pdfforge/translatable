using System;
using CommandLine;

namespace TranslationExport
{
    class Program
    {
        static void Main(string[] args)
        {
            // sample command line: --outputpath="..\..\..\TranslationTest\Languages\en-US\LC_MESSAGES" ..\..\..\TranslationTest\bin\Debug\TranslationTest.exe

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
