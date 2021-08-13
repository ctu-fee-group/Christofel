using System.Xml.Serialization;

namespace Kos.Data
{
    public enum KosProgrammeType
    {
        [XmlEnum("BACHELOR")] Bachelor,
        [XmlEnum("DOCTORAL")] Doctoral,
        [XmlEnum("INTERNSHIP")] Internship,
        [XmlEnum("LIFELONG")] Lifelong,
        [XmlEnum("MASTER")] Master,
        [XmlEnum("MASTER_LEGACY")] MasterLegacy,
        [XmlEnum("UNDEFINED")] Undefined
    }
}