[Setup]
AppName=Flash Multi
AppVersion={#MyAppVersion}
AppPublisher=Ben Lye
AppPublisherURL=https://github.com/benlye/flash-multi
DefaultDirName={commonpf}\FlashMulti
DefaultGroupName=Flash Multi
UninstallDisplayIcon={app}\flash-multi.exe
Compression=lzma2
SolidCompression=yes
OutputDir=flash-multi\bin
OutputBaseFilename=flash-multi-{#MyAppVersion}
InfoAfterFile=.\installer_infoafter.txt
DisableWelcomePage=no
LicenseFile=flash-multi\license.txt

[Files]
Source: "flash-multi\bin\Release\*"; Excludes: "*.pdb"; Flags: replacesameversion promptifolder recursesubdirs; DestDir: {app}; Components: main

[InstallDelete]
Type: filesandordirs; Name: "{app}\bin"

[Icons]
Name: "{group}\Flash Multi"; Filename: "{app}\flash-multi.exe"; IconFilename: "{app}\flash-multi.exe"; Components: main

[Components]
Name: "main"; Description: "Flash Multi Application"; Types: full compact custom; Flags: fixed
Name: "drivers"; Description: "Run Maple USB Driver Installer"; Types: full

[Run]
; Install the drivers - https://github.com/pbatard/libwdi/blob/master/examples/wdi-simple.iss
Filename: "{app}\drivers\wdi-simple.exe"; Flags: "runhidden"; Parameters: " --vid 0x1EAF --pid 0x0003 --type 2 --name ""Maple DFU"" --dest ""{tmp}\maple-dfu"" --progressbar={wizardhwnd} --timeout 120000"; StatusMsg: "Installing Maple DFU device driver (this may take a few seconds) ..."; Components: drivers
Filename: "{app}\drivers\wdi-simple.exe"; Flags: "runhidden"; Parameters: " --vid 0x1EAF --pid 0x0004 --type 3 --name ""Maple Serial"" --dest ""{tmp}\maple-serial"" --progressbar={wizardhwnd} --timeout 120000"; StatusMsg: "Installing Maple Serial device driver (this may take a few seconds) ..."; Components: drivers
