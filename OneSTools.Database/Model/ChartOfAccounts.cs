using System.Collections.Generic;

namespace OneSTools.Config
{
    public class ChartOfAccounts : MetadataObject
    {
        public List<Requisite> AccountingFlags { get; set; } = new List<Requisite>();
        public List<Requisite> ExtDimensionAccountingFlags { get; set; } = new List<Requisite>();
        public List<Requisite> Requisities { get; set; } = new List<Requisite>();
        public List<TabularSection> TabularSections { get; set; } = new List<TabularSection>();
    }
}
