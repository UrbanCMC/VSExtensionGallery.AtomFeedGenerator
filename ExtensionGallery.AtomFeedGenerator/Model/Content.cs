using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Content
	{
		[XmlAttribute("type")]
		public string Type;
		[XmlAttribute("src")]
		public string Src;
	}
}
