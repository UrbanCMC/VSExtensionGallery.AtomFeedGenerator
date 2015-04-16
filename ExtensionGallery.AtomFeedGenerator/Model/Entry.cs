using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Entry
	{
		[XmlElement("id")]
		public string Id;
		[XmlElement("title")]
		public Title Title;
		[XmlElement("summary")]
		public Summary Summary;
		[XmlElement("author")]
		public Author Author;
		[XmlElement("category")]
		public Category Category;
		[XmlElement("content")]
		public Content Content;
		[XmlElement("Vsix")]
		public Vsix Vsix;
	}
}
