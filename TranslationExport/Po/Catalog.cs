using System.Collections.Generic;
using System.Linq;

namespace TranslationExport.Po
{
    public class Catalog
    {
        public IList<PoEntry> Entries { get; } = new List<PoEntry>();

        public void AddEntry(string msgid, string comment = "", string sourceReference = "")
        {
            var entry = CreateOrGetEntry(msgid);
            UpdateEntry(entry, comment, sourceReference);
        }

        private void UpdateEntry(PoEntry entry, string comment, string sourceReference)
        {
            if (string.IsNullOrWhiteSpace(entry.Comment))
                entry.Comment = comment;

            if (!string.IsNullOrWhiteSpace(sourceReference))
                entry.SourceReferences.Add(sourceReference);
        }

        private PoEntry CreateOrGetEntry(string msgid)
        {
            var entry = Entries
                .Where(x => x is SingularEntry)
                .Cast<SingularEntry>()
                .FirstOrDefault(x => x.MsgId == msgid);

            if (entry == null)
            {
                entry = new SingularEntry(msgid);
                Entries.Add(entry);
            }

            return entry;
        }

        public void AddPluralEntry(string msgidSingular, string msgidPlural, string comment = "",
            string sourceReference = "")
        {
            var entry = CreateOrGetPluralEntry(msgidSingular, msgidPlural);

            UpdateEntry(entry, comment, sourceReference);
        }

        private PoEntry CreateOrGetPluralEntry(string msgidSingular, string msgidPlural)
        {
            var entry =
                Entries.Where(x => x is PluralEntry)
                    .Cast<PluralEntry>()
                    .FirstOrDefault(x => (x.MsgIdSingular == msgidSingular) && (x.MsgIdPlural == msgidPlural));

            if (entry == null)
            {
                entry = new PluralEntry(msgidSingular, msgidPlural);
                Entries.Add(entry);
            }

            return entry;
        }
    }
}
