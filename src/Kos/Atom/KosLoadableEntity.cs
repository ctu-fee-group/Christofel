using System;
using System.Xml.Serialization;

namespace Kos.Atom
{
    [Serializable]
    public record KosLoadableEntity(
        [property: XmlAttribute("href", Namespace = "http://www.w3.org/1999/xlink")]
        string? Href,
        [property: XmlText] string? Title
    )
    {
        public KosLoadableEntity()
            : this(default, default)
        {
        }
    }

    /// <summary>
    /// Entity holding information needed to load referenced entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public record KosLoadableEntity<T> : KosLoadableEntity
        where T : new();
}