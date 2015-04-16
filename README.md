# ExtensionGallery.AtomFeedGenerator

## Description
The ExtensionGallery.AtomFeedGenerator is used to create an atom.xml file which is used to tell Visual Studio what the content of a local extension
gallery is.
This program was created because I didn't want to bother with modifying an atom.xml file manually every time I updated an extension of mine.

## Compiling AtomFeedGenerator

You need:

	• Microsoft .NET Framework v4.0 or higher
	
All other dependencies can be found in the packages.config and should be downloaded automatically by Visual Studio.  
This requires a working internet connection and [NuGet](http://docs.nuget.org/docs/start-here/installing-nuget) to be installed.

## Usage

There are 2 ways this program can be used.  
1)	It can be started by simply opening it, which will result in the program treating the current working directory as the root folder of the extension gallery.  
2)	If it is executed using the command line, the user can add a path. This way, the specified path will be used as the root folder, so it is possible to store the program in an separate location, instead of at the root of the extension gallery.

### Parameters
The only parameters that are supported by this program are:

	• A path, which specifies where the root of the extension gallery is supposed to be.
	• [/?], [--help], [--h], which will display usage information for this application.
	
This could look like this:
ExtensionGallery.AtomFeedGenerator.exe "C:\MyExtensionGallery\".

### Folder Hierarchy
This program requires a very specific folder hierarchy to operate correctly.

The root folder is the location where the atom.xml file will be generated.  
In this folder, there is supposed to be at least one other folder, which is the category name that will be shown for the extensions inside it.

Inside that category folder, the .vsix extensions must be placed.

If any other folder hirarchy is used, the program will not correctly find your extensions and write them to the atom.xml.

## License
All Code released under [the MIT license](https://github.com/urbancmc/vsextensiongallery.atomfeedgenerator/blob/master/LICENSE).  
For third-party libraries see their respective license.