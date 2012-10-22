set VER=0.6.0

rd /s /q backend
rd /s /q package
del /q /f livereload-%VER%.tar

"%~dp0res/curl" -O http://download.livereload.com/npm/livereload-%VER%.tgz
"%~dp0res/7za" x livereload-%VER%.tgz
"%~dp0res/7za" x livereload-%VER%.tar
del livereload-%VER%.tar
ren package backend

del /q res\bundled\backend.7z
"%~dp0%res\7za.exe" a res/bundled/backend.7z backend
