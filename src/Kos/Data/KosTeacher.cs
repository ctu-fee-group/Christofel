using System.Xml.Serialization;
using Kos.Atom;

namespace Kos.Data
{
    [XmlType("teacher", Namespace = "http://kosapi.feld.cvut.cz/schema/3")]
    public record KosTeacher(
        [property: XmlElement("division")] KosLoadableEntity<KosDivision>? Division,
        [property: XmlElement("email")] string? Email,
        [property: XmlElement("extern")] bool Extern,
        [property: XmlElement("firstName")] string FirstName,
        [property: XmlElement("lastName")] string LastName,
        [property: XmlElement("personalNumber")] string PersonalNumber,
        [property: XmlElement("phone")] string? Phone,
        [property: XmlElement("stageName")] string? StageName,
        //[property: XmlElement("supervisionPhDStudents")] KosPermission SupervisionPhDStudents,
        [property: XmlElement("titlesPost")] string? TitlesPost,
        [property: XmlElement("titlesPre")] string? TitlesPre
    ) {
        public KosTeacher()
            : this(default!, default!, default!, default!,
                default!, default!, default!, default!,
                default!, default!)
        {
            
        }
    }
}