using System.Xml.Serialization;

namespace ExtensionGallery.AtomFeedGenerator.Model
{
	internal class Category
	{
		[XmlAttribute("term")]
		internal string Term;
	}
}
