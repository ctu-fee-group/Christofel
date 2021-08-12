using System.Xml.Serialization;
using Kos.Atom;

namespace Kos.Data
{
    [XmlType("programme", Namespace = "http://kosapi.feld.cvut.cz/schema/3")]
    public record KosProgramme(
        [property: XmlElement("academicTitle")]
        string AcademicTitle,
        [property: XmlElement("capacity")] ushort Capacity,
        //[property: XmlElement("classesLang")] KosClassesLang ClassesLang,
        [property: XmlElement("code")] string Code,
        [property: XmlElement("description")] string Description,
        [property: XmlElement("diplomaName")] string DiplomaName,
        [property: XmlElement("division")] KosLoadableEntity<KosDivision> Division,
        [property: XmlElement("guarantor")] KosLoadableEntity<KosTeacher> Guarantor,
        [property: XmlElement("name")] string Name,
        [property: XmlElement("openForAdmission")]
        bool OpenForAdmission,
        [property: XmlElement("studyDuration")]
        double StudyDuration,
        [property: XmlElement("type")] KosProgrammeType ProgrammeType
    )
    {
        public KosProgramme()
            : this(default!, default!, default!, default!, default!,
                default!, default!, default!, default!, default!,
                default!)
        {
        }
    }
}