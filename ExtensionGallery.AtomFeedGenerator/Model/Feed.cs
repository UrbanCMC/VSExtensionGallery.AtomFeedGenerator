using System.Collections.Generic;
using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	[XmlRoot("feed", Namespace = "http://www.w3.org/2005/Atom")]
	internal class Feed
	{
		[XmlElement("entry")]
		internal List<Entry> Entries;
	}
}
