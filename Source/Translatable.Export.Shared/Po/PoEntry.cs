using System.Collections.Generic;

namespace Translatable.Export.Shared.Po
{
    public class PoEntry
    {
        public readonly IList<string> SourceReferences = new List<string>();
        public string Context { get; set; }
        public string Comment { get; set; }
    }

    public class SingularEntry : PoEntry
    {
        public SingularEntry(string msgId, string context = "", string comment = "")
        {
            MsgId = msgId;
            Context = context ?? "";
            Comment = comment ?? "";
        }

        public string MsgId { get; }
    }

    public class PluralEntry : PoEntry
    {
        public PluralEntry(string msgIdSingular, string msgIdPlural, string context = "", string comment = "")
        {
            MsgIdSingular = msgIdSingular;
            MsgIdPlural = msgIdPlural;
            Context = context ?? "";
            Comment = comment ?? "";
        }

        public string MsgIdSingular { get; }
        public string MsgIdPlural { get; }
    }
}