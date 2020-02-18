using System;
using System.Collections.Generic;
using System.Text;

namespace OneSTools.Config
{
    public class ReferenceTypeInfo : TypeInfo
    {
        public string UUID { get; set; }

        public ReferenceTypeInfo(string uuid) : base(ValueType.Binary)
        {
            UUID = uuid;
        }
    }
}
