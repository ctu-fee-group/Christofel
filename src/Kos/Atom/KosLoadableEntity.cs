using System;
using System.Xml.Serialization;

namespace Kos.Atom
{
    [Serializable]
    public record LoadableEntity(
        [property: XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        string? Href,
        [property: XmlText] string? Title
    )
    {
        public LoadableEntity()
            : this(default, default)
        {
        }
    }

    public record KosLoadableEntity<T> : LoadableEntity
        where T : new();
}