using System.Collections.Generic;
using System.Xml.Serialization;
using Kos.Atom;

namespace Kos.Data
{
    public record KosPersonRoles(
        [property: XmlElement("student")] List<KosLoadableEntity<KosStudent>> Students,
        [property: XmlElement("teacher")] KosLoadableEntity<KosTeacher>? Teacher)
    {
        public KosPersonRoles()
            : this(default!, null)
        {
            
        }
    }
}