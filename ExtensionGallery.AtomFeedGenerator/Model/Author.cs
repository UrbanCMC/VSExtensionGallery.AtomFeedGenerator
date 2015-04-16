using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Author
	{
		[XmlElement("name")]
		public string Name;
	}
}
