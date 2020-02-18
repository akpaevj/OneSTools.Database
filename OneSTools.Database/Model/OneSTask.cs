using System.Collections.Generic;

namespace OneSTools.Config
{
    public class OneSTask : MetadataObject
    {
        public List<Requisite> AddressingAttributes { get; set; } = new List<Requisite>();
        public List<Requisite> Requisities { get; set; } = new List<Requisite>();
        public List<TabularSection> TabularSections { get; set; } = new List<TabularSection>();
    }
}
