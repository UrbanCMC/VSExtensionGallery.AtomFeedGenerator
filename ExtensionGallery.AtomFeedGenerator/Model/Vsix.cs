using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Vsix
	{
		[XmlElement("Id")]
		public string Id;
		[XmlElement("Version")]
		public string Version;
	}
}
