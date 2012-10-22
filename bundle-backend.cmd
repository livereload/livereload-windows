rd /s /q backend
rd /s /q package
del /q /f livereload-0.2.0.tar

: "%~dp0res/curl" -O http://download.livereload.com/npm/livereload-0.2.0.tgz
"%~dp0res/7za" x livereload-0.2.0.tgz
"%~dp0res/7za" x livereload-0.2.0.tar
del livereload-0.2.0.tar
ren package backend

del /q res\bundled\backend.7z
"%~dp0%res\7za.exe" a res/bundled/backend.7z backend
