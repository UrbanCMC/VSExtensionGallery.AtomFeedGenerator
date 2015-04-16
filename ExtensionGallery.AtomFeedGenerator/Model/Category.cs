using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	public class Category
	{
		[XmlAttribute("term")]
		public string Term;
	}
}
