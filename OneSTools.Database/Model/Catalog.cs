using System.Collections.Generic;

namespace OneSTools.Config
{
    public class Catalog : MetadataObject
    {
        public bool HasOwners { get; set; }
        public List<Requisite> Requisities { get; set; } = new List<Requisite>();
        public List<TabularSection> TabularSections { get; set; } = new List<TabularSection>();
    }
}
