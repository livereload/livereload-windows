set VER=0.8.12

cd "%~dp0"

rd /s /q node

res\curl -O "http://nodejs.org/dist/v%VER%/node.exe"

md "node-%VER%"
move node.exe "node-%VER%\LiveReloadNodejs.exe"

del /q /f "res\bundled\node-%VER%.7z"
res\7za.exe a "res\bundled\node-%VER%.7z" "node-%VER%"

rd /s /q "node-%VER%"
