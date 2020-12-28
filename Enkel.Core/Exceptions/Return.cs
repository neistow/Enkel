namespace Enkel.Core.Exceptions
{
    public class Return : EnkelException
    {
        public object Value { get; }

        public Return(object value) : base("")
        {
            Value = value;
        }
    }
}