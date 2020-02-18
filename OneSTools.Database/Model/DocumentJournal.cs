using System.Collections.Generic;

namespace OneSTools.Config
{
    public class DocumentJournal : MetadataObject
    {
        public List<Requisite> Graphs { get; set; } = new List<Requisite>();
    }
}
