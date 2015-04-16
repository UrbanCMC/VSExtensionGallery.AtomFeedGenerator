using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Entry
	{
		[XmlElement("id")]
		internal string Id;
		[XmlElement("title")]
		internal Title Title;
		[XmlElement("summary")]
		internal Summary Summary;
		[XmlElement("author")]
		internal Author Author;
		[XmlElement("category")]
		internal Category Category;
		[XmlElement("content")]
		internal Content Content;
		[XmlElement("Vsix")]
		internal Vsix Vsix;
	}
}
