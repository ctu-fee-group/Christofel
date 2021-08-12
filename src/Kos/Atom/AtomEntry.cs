using System;
using System.Xml.Serialization;
using Kos.Data;

namespace Kos.Atom
{
    [XmlRoot("entry", Namespace = "http://www.w3.org/2005/Atom"), Serializable]
    public class AtomEntry<T>
    {
        [XmlElement("title")]
        public string Title { get; set; }
        
        [XmlElement("id")]
        public string Id { get; set; }
        
        [XmlElement("updated")]
        public DateTime Updated { get; set; }

        [XmlElement("content")]
        public T? Content { get; set; }
    }
}