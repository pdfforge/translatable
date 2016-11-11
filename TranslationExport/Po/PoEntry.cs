using System.Collections.Generic;

namespace TranslationExport.Po
{
    public class PoEntry
    {
        public string Comment { get; set; }
        public IList<string> SourceReferences = new List<string>();

    }

    public class SingularEntry : PoEntry
    {
        public SingularEntry(string msgId, string comment = "")
        {
            MsgId = msgId;
            Comment = comment ?? "";
        }

        public string MsgId { get; }
    }

    public class PluralEntry : PoEntry
    {
        public PluralEntry(string msgIdSingular, string msgIdPlural, string comment = "")
        {
            MsgIdSingular = msgIdSingular;
            MsgIdPlural = msgIdPlural;
            Comment = comment ?? "";
        }

        public string MsgIdSingular { get; }
        public string MsgIdPlural { get; }
    }
}
