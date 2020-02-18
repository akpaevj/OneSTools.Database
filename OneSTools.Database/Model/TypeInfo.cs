using System;
using System.Collections.Generic;
using System.Text;

namespace OneSTools.Config
{
    public class TypeInfo
    {
        public ValueType Type { get; set; }

        public TypeInfo(ValueType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }
}
