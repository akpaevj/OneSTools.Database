namespace OneSTools.Config
{
    public class StringTypeInfo : TypeInfo
    {
        public bool FixedLength { get; set; }
        public int Length { get; set; }

        public StringTypeInfo(int length, bool fixedLength) : base(ValueType.String)
        {
            Length = length;
            FixedLength = fixedLength;
        }
    }
}
