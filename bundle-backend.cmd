set VER=0.7.0

cd "%~dp0"

rd /s /q backend
rd /s /q package
del /q /f "livereload-%VER%.tar"

tools\curl -O "http://download.livereload.com/npm/livereload-%VER%.tgz"
tools\7za x "livereload-%VER%.tgz"
tools\7za x "livereload-%VER%.tar"
del /q /f "livereload-%VER%.tar"
ren package backend

for /d /r . %%d in (test example examples) do @if exist "%%d" rd /s/q "%%d"

del /q res\bundled\backend.7z
tools\7za.exe a res\bundled\backend.7z backend
