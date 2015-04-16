using System;
using ExtensionGallery.AtomFeedGenerator.XML;

namespace ExtensionGallery.AtomFeedGenerator
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				if (args.Length != 0)
				{
					if (args[0] == "/?" || args[0] == "--help" || args[0] == "--h")
					{
						PrintHelp();
					}
					else
					{
						new AtomFeed(args[0]).Generate();
					}
				}
				else
				{
					new AtomFeed().Generate();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			
		}

		private static void PrintHelp()
		{
			Console.WriteLine("Usage: ExtensionGallery.AtomFeedGenerator.exe [ExtensionGalleryPath]");
			Console.WriteLine("[ExtensionGalleryPath] => The path to the root of the extension gallery.");
			Console.WriteLine("		If it is not specified, the directory where the program is located");
			Console.WriteLine("		will be used instead.");
		}
	}
}