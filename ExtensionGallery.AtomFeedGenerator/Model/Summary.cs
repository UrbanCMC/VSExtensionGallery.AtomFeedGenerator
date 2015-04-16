using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Summary
	{
		[XmlAttribute("type")]
		public string Type;
		[XmlText]
		public string Text;
	}
}
