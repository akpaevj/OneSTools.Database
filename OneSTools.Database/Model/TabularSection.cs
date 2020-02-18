using System.Collections.Generic;

namespace OneSTools.Config
{
    public class TabularSection : MetadataObject
    {
        public List<Requisite> Requisities { get; set; } = new List<Requisite>();
    }
}
