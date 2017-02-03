using System.Collections.Generic;
using System.Linq;

namespace Translatable.Export.Po
{
    public class Catalog
    {
        public IList<PoEntry> Entries { get; } = new List<PoEntry>();

        public void AddEntry(string msgid, string comment = "", string sourceReference = "", string context = "")
        {
            var entry = CreateOrGetEntry(msgid, context);
            UpdateEntry(entry, comment, sourceReference);
        }

        private void UpdateEntry(PoEntry entry, string comment, string sourceReference)
        {
            if (string.IsNullOrWhiteSpace(entry.Comment))
                entry.Comment = comment;

            if (!string.IsNullOrWhiteSpace(sourceReference))
                entry.SourceReferences.Add(sourceReference);
        }

        private PoEntry CreateOrGetEntry(string msgid, string context)
        {
            var entry = Entries
                .Where(x => x is SingularEntry)
                .Cast<SingularEntry>()
                .FirstOrDefault(x => x.MsgId == msgid && x.Context == context);

            if (entry == null)
            {
                entry = new SingularEntry(msgid, context);
                Entries.Add(entry);
            }

            return entry;
        }

        public void AddPluralEntry(string msgidSingular, string msgidPlural, string context, string comment = "",
            string sourceReference = "")
        {
            var entry = CreateOrGetPluralEntry(msgidSingular, msgidPlural, context);

            UpdateEntry(entry, comment, sourceReference);
        }

        private PoEntry CreateOrGetPluralEntry(string msgidSingular, string msgidPlural, string context)
        {
            var entry =
                Entries.Where(x => x is PluralEntry)
                    .Cast<PluralEntry>()
                    .FirstOrDefault(x => (x.MsgIdSingular == msgidSingular) && (x.MsgIdPlural == msgidPlural) && (x.Context == context));

            if (entry == null)
            {
                entry = new PluralEntry(msgidSingular, msgidPlural, context);
                Entries.Add(entry);
            }

            return entry;
        }
    }
}
