using System.Collections.Generic;
using System.Linq;
using Christofel.BaseLib.Extensions;

namespace Christofel.BaseLib.Configuration.Converters
{
    public class IEnumerableConverter<TElement> : ConfigConverter<IEnumerable<TElement>>
    {
        public IEnumerableConverter(string separator = ";")
        {
            Separator = separator;
        }
        
        public string Separator { get; }
        
        public override string? GetString(IEnumerable<TElement> value, IConfigConverterResolver resolver)
        {
            IConfigConverter converter = resolver.GetConverter<TElement>();

            return string.Join(Separator, value.Select(x => converter.GetString(x, resolver)));
        }

        public override IEnumerable<TElement> Convert(string value, IConfigConverterResolver resolver)
        {
            IConfigConverter converter = resolver.GetConverter<TElement>();

            return value.Split(Separator)
                .Select(x => converter.Convert(x, resolver))
                .Cast<TElement>();
        }
    }
}