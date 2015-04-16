using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Author
	{
		[XmlElement("name")]
		internal string Name;
	}
}
