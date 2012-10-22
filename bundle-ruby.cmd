set ROOT=%CD%
cd res/bundled

: Download
: rm -f ruby-1.9.3-p286-i386-mingw32.7z
: curl -LO http://rubyforge.org/frs/download.php/76528/ruby-1.9.3-p286-i386-mingw32.7z
@if errorlevel 1 goto error

: Extract
rm -rf ruby-1.9.3-p286-i386-mingw32
%ROOT%\res\7za x ruby-1.9.3-p286-i386-mingw32.7z
@if errorlevel 1 goto error

: Strip
rm -rf ruby-1.9.3
mv ruby-1.9.3-p286-i386-mingw32 ruby-1.9.3
cd ruby-1.9.3
rm -rf bin/tcl*
rm -rf bin/tk*
rm -rf lib/tcltk
rm -rf lib/*.a
rm -rf lib/ruby/1.9.1/tk*
cd ..

: Compress
rm -f ruby-1.9.3.7z
%ROOT%\res\7za a ruby-1.9.3.7z ruby-1.9.3
@if errorlevel 1 goto error

: Cleanup
rm -rf ruby-1.9.3

echo Success.
goto :eof

:error
@echo off
echo Failed.
exit /b 1
