using System.Collections.Generic;
using CommandLine;

namespace Translatable.Export.Shared
{
    public class ExportOptions
    {
        [Value(0, HelpText = "Assembly files that will be scanned for ITranslatable classes.", MetaName = "AssemblyFiles", Required = true)]
        public IEnumerable<string> Assemblies { get; set; }

        [Option(Required = true, HelpText = "The path to the output pot file. If it does not exist, it will be created.")]
        public string OutputFile { get; set; }
    }
}