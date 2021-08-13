using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Kos.Atom;

namespace Kos.Data
{
    [XmlType("person", Namespace = "http://kosapi.feld.cvut.cz/schema/3")]
    public record KosPerson(
        [property: XmlElement("firstName")] string FirstName,
        [property: XmlElement("lastName")] string LastName,
        [property: XmlElement("personalNumber")]
        string PersonalNumber,
        [property: XmlElement("roles")] KosPersonRoles Roles,
        [property: XmlElement("titlesPre")] string? TitlesPre,
        [property: XmlElement("titlesPost")] string? TitlesPost,
        [property: XmlElement("username")] string Username
    )
    {
        public KosPerson() : this(default!, default!, default!, default!, default!, default!, default!)
        {
        }
    }
}