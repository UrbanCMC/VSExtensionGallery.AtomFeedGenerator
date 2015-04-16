using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Content
	{
		[XmlAttribute("type")]
		internal string Type;
		[XmlAttribute("src")]
		internal string Src;
	}
}
