using System.Collections.Generic;

namespace OneSTools.Config
{
    public class CalculationRegister : MetadataObject
    {
        public List<Requisite> Requisities { get; set; } = new List<Requisite>();
        public List<Requisite> Dimensions { get; set; } = new List<Requisite>();
        public List<Requisite> Resources { get; set; } = new List<Requisite>();
        public List<Recalculation> Recalculations { get; set; } = new List<Recalculation>();
    }
}
