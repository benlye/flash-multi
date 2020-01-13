@ECHO OFF

REM Import the Visual Studio command line build environment, remembering the current working directory
PUSHD %cd%
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsMSBuildCmd.bat" (
  CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\Common7\Tools\VsDevCmd.bat"
  POPD
  GOTO BUILD
)
IF EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsMSBuildCmd.bat" (
  CALL "%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\Common7\Tools\VsMSBuildCmd.bat"
  POPD
  GOTO BUILD
)

ECHO Couldn't find Visual Studio tools
GOTO :EOF

:BUILD
REM Delete any existing build output
rmdir /s /q .\flash-multi\bin

REM Run the build command
msbuild /t:Clean /p:Configuration=Release
msbuild /t:Build /p:Configuration=Release

REM Get the version
FOR /F "USEBACKQ" %%F IN (`powershell -NoLogo -NoProfile -Command ^(Get-Item ".\flash-multi\bin\Release\flash-multi.exe"^).VersionInfo.FileVersion`) DO (SET fileVersion=%%F)
echo File version: %fileVersion%

REM Create the zip archive
"C:\Program Files\7-Zip\7z.exe" a -r -x!*.pdb .\flash-multi\bin\flash-multi-%fileVersion%.zip .\flash-multi\bin\Release

REM Rename the root folder in the zip
"C:\Program Files\7-Zip\7z.exe" rn  .\flash-multi\bin\flash-multi-%fileVersion%.zip Release\ flash-multi-%fileVersion%\

REM Create the installer package
REM "C:\Program Files (x86)\Inno Setup 6\iscc.exe"  /Qp /DMyAppVersion=%fileVersion% flash-multi.iss
"C:\Program Files (x86)\NSIS\Bin\makensis.exe" /DVERSION=%fileVersion% flash-multi.nsi

REM Get the SH256 hashes from the files
ECHO.
FOR /F "USEBACKQ" %%F IN (`powershell -NoLogo -NoProfile -Command ^(Get-FileHash ".\flash-multi\bin\flash-multi-%fileVersion%.zip"^).Hash`) DO (SET fileHash=%%F)
ECHO flash-multi-%fileVersion%.zip: %fileHash%

FOR /F "USEBACKQ" %%F IN (`powershell -NoLogo -NoProfile -Command ^(Get-FileHash ".\flash-multi\bin\flash-multi-%fileVersion%.exe"^).Hash`) DO (SET fileHash=%%F)
ECHO flash-multi-%fileVersion%.exe: %fileHash%
