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
                    output.WriteLine("\"Content-Type: text/plain; charset=UTF-8\"");
                    output.WriteLine();

                    foreach (var poEntry in catalog.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(poEntry.Comment))
                            output.WriteLine($"#. {poEntry.Comment}");

                        if (poEntry.SourceReferences.Any())
                        {
                            var referencesString = string.Join(" ", poEntry.SourceReferences);
                            output.WriteLine($"#: {referencesString}");
                        }

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
    }
}
