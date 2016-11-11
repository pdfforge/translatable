using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationExport.Po
{
    public class Catalog
    {
        public IList<PoEntry> Entries { get; } = new List<PoEntry>();

        public void AddEntry(string msgid, string comment = "")
        {
            var existingEntry = Entries.Where(x => x is SingularEntry).Cast<SingularEntry>().FirstOrDefault(x => x.MsgId == msgid);

            if (existingEntry != null)
            {
                if (string.IsNullOrWhiteSpace(existingEntry.Comment))
                    existingEntry.Comment = comment;
                return;
            }

            Entries.Add(new SingularEntry(msgid, comment));
        }

        public void AddPluralEntry(string msgidSingular, string msgidPlural, string comment = "")
        {
            var existingEntry = Entries.Where(x => x is PluralEntry).Cast<PluralEntry>().FirstOrDefault(x => x.MsgIdSingular == msgidSingular && x.MsgIdPlural == msgidPlural);

            if (existingEntry != null)
            {
                if (string.IsNullOrWhiteSpace(existingEntry.Comment))
                    existingEntry.Comment = comment;
                return;
            }

            Entries.Add(new PluralEntry(msgidSingular, msgidPlural, comment));
        }
    }
}
