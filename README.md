livereload-windows
==================

LiveReload for Windows


## Building

Prerequisites:

* Git installed with mingw tools (rm, curl, etc) added to PATH
* Microsoft Visual Studio 2010

Process:

* Run bundle-backend.cmd
* Run bundle-ruby.cmd
* Perform the build in Visual Studio


## Bundled Ruby

We use 7z archive of Ruby downloaded via http://rubyinstaller.org/.

Ruby 1.9.3 is ruby-1.9.3-p286-i386-mingw32.7z with the following items removed:

	bin/tcl*
	bin/tk*
	lib/tcltk/
	lib/ruby/1.9.1/tk*
	lib/*.a   # presumably these are only needed to build native extensions?


## Acknowledgements

* fastJSON library:      http://fastjson.codeplex.com/
* MahApps.Metro:         http://mahapps.com/MahApps.Metro/
* Modern UI Icons:       http://modernuiicons.com/

NOTES:

* when updating fastJSON library to newer version, change its Target Framework to match one of LiveReload.
