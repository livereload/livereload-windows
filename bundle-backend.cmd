del /s /q backend
del /s /q node_modules
call npm install http://download.livereload.com/npm/livereload-0.2.0.tgz
move node_modules/livereload backend
del /q res/bundled/backend.7z
"%~dp0%res\7za.exe" a res/bundled/backend.7z backend
