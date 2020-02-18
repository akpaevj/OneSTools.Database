using System;

namespace OneSTools.Config
{
    public abstract class MetadataObject
    {
        public string UUID { get; set; }
        public string Name { get; set; }
        public string Synonym { get; set; }
        public int Number { get; set; }
        public string SDBL { get; set; }

        public override string ToString()
        {
            return Name + (string.IsNullOrEmpty(SDBL) ? "" : $" ({SDBL})");
        }
    }
}
