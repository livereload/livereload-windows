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


## Acknowledgements

* fastJSON library:      http://fastjson.codeplex.com/
* MahApps.Metro:         http://mahapps.com/MahApps.Metro/
* Modern UI Icons:       http://modernuiicons.com/

NOTES:

* when updating fastJSON library to newer version, change its Target Framework to match one of LiveReload.
