using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Title
	{
		[XmlAttribute("type")]
		public string Type;
		[XmlText]
		public string Text;
	}
}
