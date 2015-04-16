using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	[XmlRoot("feed", Namespace = "http://www.w3.org/2005/Atom")]
	public class Feed
	{
		[XmlElement("entry")]
		public List<Entry> Entries;
	}
}
