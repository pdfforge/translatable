using System.Collections.Generic;
using CommandLine;

namespace TranslationExport
{
    class ExportOptions
    {
        [Value(0, Min = 1, HelpText = "Assembly files that will be scanned for ITranslatable classes.")]
        public IEnumerable<string> Assemblies { get; set; }

        [Option(Required = true, HelpText = "The output folder, where the po and pot files will be written.")]
        public string OutputPath { get; set; }
    }
}
