namespace Christofel.BaseLib.Configuration.Converters
{
    public class ConvertConfigConverter<T> : ConfigConverter<T>
    {
        public override T Convert(string value)
        {
            return (T)System.Convert.ChangeType(value, typeof(T));
        }
    }
}