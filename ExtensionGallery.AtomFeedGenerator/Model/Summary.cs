using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Summary
	{
		[XmlAttribute("type")]
		internal string Type;
		[XmlText]
		internal string Text;
	}
}
