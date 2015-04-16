using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Xml.Linq;
using System.Xml.Serialization;
using ExtensionGallery.AtomFeedGenerator.Model;

namespace ExtensionGallery.AtomFeedGenerator.XML
{
	internal class AtomFeed
	{
		#region Private

		#region - Vars

		private readonly string _extensionGalleryPath;
		private Stream _extensionXmlStream;

		#endregion

		#region - Methods

		/// <summary>
		/// Loads the XML data from the extension.vsixmanifest file located inside the extension package.
		/// </summary>
		/// <param name="vsixPath">The path to the extension package.</param>
		/// <returns>A dictionary of <see cref="ExtensionDataType"/> and their value, which contains the information retrieved from the extension.</returns>
		private Dictionary<ExtensionDataType, string> LoadExtensionXml(string vsixPath)
		{
			var extensionData = new Dictionary<ExtensionDataType, string>();
			var xmlZipResolver = new XmlVsixResolver();
			this._extensionXmlStream = (Stream)xmlZipResolver.GetEntity(new Uri(String.Format("vsix:{0}!/extension.vsixmanifest", vsixPath)), "", extensionData.GetType());
			var extensionDocument = XDocument.Load(this._extensionXmlStream);

			if (extensionDocument == null)
			{
				throw new InvalidOperationException(String.Format("Unable to load extension.vsixmanifest for {0}", vsixPath));
			}

			if (extensionDocument.Root == null || !extensionDocument.Root.HasElements)
			{
				throw new InvalidOperationException(String.Format("Invalid XML syntax in extension.vsixmanifest for {0}", vsixPath));
			}

			extensionData.Add(ExtensionDataType.Id, ((XElement) extensionDocument.Root.Elements().First().FirstNode).Attribute("Id").Value);
			extensionData.Add(ExtensionDataType.Version, ((XElement)extensionDocument.Root.Elements().First().FirstNode).Attribute("Version").Value);
			extensionData.Add(ExtensionDataType.Publisher, ((XElement)extensionDocument.Root.Elements().First().FirstNode).Attribute("Publisher").Value);
			extensionData.Add(ExtensionDataType.DisplayName, (extensionDocument.Root.Elements().First().Elements().ElementAt(1)).Value);
			extensionData.Add(ExtensionDataType.Description, (extensionDocument.Root.Elements().First().Elements().ElementAt(2)).Value);

			return extensionData;
		}

		/// <summary>
		/// Generates an entry for an extension, which will be placed in the atom.xml
		/// </summary>
		/// <param name="category">The category to which the extension belongs</param>
		/// <param name="filePath">The path to the extension file.</param>
		/// <returns>An <see cref="Entry"/> containing information about the specified extension that will be displayed in the extensionGallery
		/// of Visual Studio</returns>
		private Entry GenerateEntry(string category, string filePath)
		{
			var entry = new Entry();
			var extensionData = this.LoadExtensionXml(filePath);

			entry.Id = extensionData[ExtensionDataType.Id];
			entry.Title = new Title()
			{
				Type = "text",
				Text = extensionData[ExtensionDataType.DisplayName]
			};
			entry.Summary = new Summary()
			{
				Type = "text",
				Text = extensionData[ExtensionDataType.Description]
			};
			entry.Author = new Author()
			{
				Name = extensionData[ExtensionDataType.Publisher]
			};
			entry.Category = new Category()
			{
				Term = category
			};
			entry.Content = new Content()
			{
				Type = "application/octet-stream",
				Src = String.Format("{0}/{1}", category, Path.GetFileName(filePath))
			};
			entry.Vsix = new Vsix()
			{
				Id = extensionData[ExtensionDataType.Id],
				Version = extensionData[ExtensionDataType.Version]
			};

			return entry;
		}

		#endregion

		#endregion

		#region Internal

		#region - Constructors

		/// <summary>
		/// Creates a new instance of the AtomFeed class, using the current working directory as extensionGallery path
		/// </summary>
		/// <exception cref="IOException">An I/O error occurred.</exception>
		/// <exception cref="DirectoryNotFoundException">Attempted to set a local path that cannot be found.</exception>
		/// <exception cref="SecurityException">The caller does not have the appropriate permission.</exception>
		internal AtomFeed()
		{
			this._extensionGalleryPath = Environment.CurrentDirectory;
		}

		/// <summary>
		/// Creates a new instance of the AtomFeed class using the specified extensionGallery path
		/// </summary>
		/// <param name="extensionGalleryPath">The path where the extensionGallery has its root, which is also where the atom.xml will be placed.</param>
		internal AtomFeed(string extensionGalleryPath)
		{
			this._extensionGalleryPath = extensionGalleryPath;
		}

		#endregion

		#region - Methods

		/// <summary>
		/// Generates the atom.xml feed that is required for an extension gallery, by listing all extensions found in subdirectories of the
		/// extensionGallery path specified.
		/// </summary>
		/// <exception cref="UnauthorizedAccessException">The required permissions to access the directory are missing. </exception>
		/// <exception cref="PathTooLongException">The specified path, exceeds the system-defined maximum length. On Windows, this limit is 248 characters. </exception>
		/// <exception cref="IOException">The specified extensionGallery path is a file name. </exception>
		/// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
		/// <exception cref="SecurityException">The required permissions for writing the atom.xml file to disk are missing. </exception>
		internal void Generate()
		{
			var feed = new Feed();
			feed.Entries = new List<Entry>();

			var directories = Directory.GetDirectories(this._extensionGalleryPath);

			if(File.Exists(Path.Combine(this._extensionGalleryPath, "atom.xml")))
			{
				File.Delete(Path.Combine(this._extensionGalleryPath, "atom.xml"));
			}

			foreach(var directory in directories)
			{
				var dir = directory;
				var files = Directory.GetFiles(directory);

				foreach(var file in files)
				{
					//We need only the name of the current folder
					if (dir.EndsWith("\\"))
					{
						dir = dir.Substring(0, dir.Length - 1);
					}

					var lastSeparatorIndex = dir.LastIndexOf("\\", StringComparison.Ordinal) + 1;
					dir = dir.Substring(lastSeparatorIndex, dir.Length - lastSeparatorIndex);

					feed.Entries.Add(this.GenerateEntry(dir, file));
				}
			}

			var serializer = new XmlSerializer(typeof(Feed));

			// The FileNotFoundException is actually never thrown, because if the file would not exist, we would create it.
			using (var fs = new FileStream(Path.Combine(this._extensionGalleryPath, "atom.xml"), FileMode.OpenOrCreate, FileAccess.Write))
			{
				serializer.Serialize(fs, feed);
				fs.Close();
			}
		}

		#endregion

		#endregion
	}
}