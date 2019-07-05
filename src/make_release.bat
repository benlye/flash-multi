@ECHO OFF

REM Get the version
FOR /F "USEBACKQ" %%F IN (`powershell -NoLogo -NoProfile -Command ^(Get-Item ".\flash-multi\bin\Release\flash-multi.exe"^).VersionInfo.FileVersion`) DO (SET fileVersion=%%F)
echo File version: %fileVersion%

REM Delete an existing archive
DEL .\flash-multi\bin\flash-multi-%fileVersion%.zip

REM Create the zip archive
"C:\Program Files\7-Zip\7z.exe" a -r -x!*.pdb .\flash-multi\bin\flash-multi-%fileVersion%.zip .\flash-multi\bin\Release

REM Rename the root folder in the zip
"C:\Program Files\7-Zip\7z.exe" rn  .\flash-multi\bin\flash-multi-%fileVersion%.zip Release\ flash-multi-%fileVersion%\

REM Create the installer package
"C:\Program Files (x86)\Inno Setup 6\iscc.exe"  /Qp /DMyAppVersion=%fileVersion% flash-multi.iss
