using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Translatable.Export.Po
{
    public class PotWriter
    {
        public void WritePotFile(string filename, Catalog catalog)
        {
            filename = Path.GetFullPath(filename);
            var outputDirectory = Path.GetDirectoryName(filename);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            using (var s = File.Create(filename))
            {
                using (var output = new StreamWriter(s, new UTF8Encoding(false)))
                {
                    output.WriteLine("msgid \"\"");
                    output.WriteLine("msgstr \"\"");
                    output.WriteLine("\"MIME-Version: 1.0\\n\"");
                    output.WriteLine("\"Content-Type: text/plain; charset=UTF-8\\n\"");
                    output.WriteLine("\"Content-Transfer-Encoding: 8bit\\n\"");
                    output.WriteLine("\"POT-Creation-Date: YEAR-MO-DA HO:MI+ZONE\\n\"");
                    output.WriteLine("\"Language: en\\n\"");
                    output.WriteLine();

                    foreach (var poEntry in catalog.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(poEntry.Comment))
                            output.WriteLine($"#. {poEntry.Comment}");

                        foreach (var sourceReference in GetDistinctSourceReferences(poEntry))
                        {
                            output.WriteLine($"#: {sourceReference}");
                        }

                        if (!string.IsNullOrWhiteSpace(poEntry.Context))
                            output.WriteLine($"msgctxt \"{poEntry.Context}\"");

                        if (poEntry is SingularEntry)
                        {
                            var singularEntry = poEntry as SingularEntry;
                            output.WriteLine($"msgid \"{singularEntry.MsgId}\"");
                            output.WriteLine("msgstr \"\"");
                            output.WriteLine();
                        }
                        else if (poEntry is PluralEntry)
                        {
                            var pluralEntry = poEntry as PluralEntry;
                            output.WriteLine($"msgid \"{pluralEntry.MsgIdSingular}\"");
                            output.WriteLine($"msgid_plural \"{pluralEntry.MsgIdPlural}\"");
                            output.WriteLine("msgstr[0] \"\"");
                            output.WriteLine("msgstr[1] \"\"");
                            output.WriteLine();
                        }
                    }
                }
            }
        }

        private IEnumerable<string> GetDistinctSourceReferences(PoEntry poEntry)
        {
            return poEntry.SourceReferences
                .Distinct()
                .OrderBy(s => s);
        }
    }
}
