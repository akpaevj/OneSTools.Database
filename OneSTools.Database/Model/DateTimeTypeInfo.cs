namespace OneSTools.Config
{
    public class DateTimeTypeInfo : TypeInfo
    {
        public DateTimeKind Kind { get; set; }

        public DateTimeTypeInfo(DateTimeKind kind) : base(ValueType.DateTime)
        {
            Kind = kind;
        }
    }
}
