using System.Collections.Generic;

namespace OneSTools.Config
{
    public class Requisite : MetadataObject
    {
        public List<TypeInfo> Types { get; set; } = new List<TypeInfo>();
    }
}
