namespace OneSTools.Config
{
    public class NumericTypeInfo : TypeInfo
    {
        public int Length { get; set; }
        public int Precision { get; set; }
        public bool NotNegative { get; set; }

        public NumericTypeInfo(int length, int precision, bool notNegative) : base(ValueType.Numeric)
        {
            Length = length;
            Precision = precision;
        }
    }
}
