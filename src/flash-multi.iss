#define MyAppVersion '0.1.0'
[Setup]
AppName=Flash Multi
AppVersion={#MyAppVersion}
AppPublisher=Ben Lye
AppPublisherURL=https://github.com/benlye/flash-multi
DefaultDirName={pf}\FlashMulti
DefaultGroupName=Flash Multi
UninstallDisplayIcon={app}\flash-multi.exe
Compression=lzma2
SolidCompression=yes
OutputDir=flash-multi\bin
OutputBaseFilename=flash-multi-{#MyAppVersion}

[Files]
Source: "flash-multi\bin\Release\*"; Excludes: "*.pdb"; Flags: replacesameversion promptifolder recursesubdirs; DestDir: {app}

[Icons]
Name: "{group}\Flash Multi"; Filename: "{app}\flash-multi.exe"; IconFilename: "{app}\flash-multi.exe"

[Run]
; Install the drivers - https://github.com/pbatard/libwdi/blob/master/examples/wdi-simple.iss
Filename: "{app}\drivers\wdi-simple.exe"; Flags: "runhidden"; Parameters: " --vid 0x1EAF --pid 0x0003 --type 1 --name ""Maple DFU"" --dest ""{tmp}\maple-dfu"" --progressbar={wizardhwnd} --timeout 120000"; StatusMsg: "Installing Maple DFU device driver (this may take a few seconds) ...";
Filename: "{app}\drivers\wdi-simple.exe"; Flags: "runhidden"; Parameters: " --vid 0x1EAF --pid 0x0004 --type 3 --name ""Maple Serial"" --dest ""{tmp}\maple-serial"" --progressbar={wizardhwnd} --timeout 120000"; StatusMsg: "Installing Maple Serial device driver (this may take a few seconds) ...";
