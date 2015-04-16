using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Vsix
	{
		[XmlElement("Id")]
		internal string Id;
		[XmlElement("Version")]
		internal string Version;
	}
}
