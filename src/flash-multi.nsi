
# Use the modern UI
!include MUI2.nsh
!include "FileFunc.nsh"
!include "nsProcess.nsh"

; The name of the installer
Name "Flash Multi"

; The file to write
OutFile "flash-multi\bin\flash-multi-${VERSION}.exe"

; The default installation directory
InstallDir $PROGRAMFILES\FlashMulti

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\FlashMulti" "Install_Dir"

; Request application privileges
RequestExecutionLevel admin

; Installer initialization function - checks for previous installation
Function .onInit
  IfFileExists "$INSTDIR\unins000.exe" PreviousVersionWarn
  IfFileExists "$INSTDIR\uninstall.exe" PreviousVersionWarn
  Goto End

  PreviousVersionWarn:
    MessageBox MB_YESNO|MB_ICONQUESTION "Remove the existing installation of Flash Multi?$\n$\nAnswering $\"No$\" will abort the installation." /SD IDYES IDYES Uninstall
    Goto AbortInstall
      
  Uninstall:
    IfFileExists "$INSTDIR\unins000.exe" InnoUninstall
    IfFileExists "$INSTDIR\uninstall.exe" NsisUninstall
    Goto End

  InnoUninstall:
    ExecWait '"$INSTDIR\unins000.exe" /SILENT'
    Goto End

  NsisUninstall:
    ExecWait '"$INSTDIR\uninstall.exe" /S _=$INSTDIR'
    Goto End

  AbortInstall:
    abort

  End:

FunctionEnd

; Uninstaller initialization function - checks for running program
Function un.onInit
  ${nsProcess::FindProcess} "flash-multi.exe" $R0
  ${If} $R0 == 0
    MessageBox MB_OKCANCEL|MB_ICONEXCLAMATION "Flash Multi is running. Click OK to close it or Cancel to abort." /SD IDOK IDOK Close
    Abort
  ${Else}
    Goto End
  ${EndIf}

  Close:
    ${nsProcess::CloseProcess} "flash-multi.exe" $R0
    Goto End

  End:
    ${nsProcess::Unload}
FunctionEnd

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

;--------------------------------
;Pages

  !define MUI_COMPONENTSPAGE_NODESC
  !insertmacro MUI_PAGE_LICENSE ".\flash-multi\bin\Release\license.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES

  !define MUI_FINISHPAGE_LINK 'https://github.com/benlye/flash-multi/'
  !define MUI_FINISHPAGE_LINK_LOCATION https://github.com/benlye/flash-multi/
  !define /file MUI_FINISHPAGE_TEXT ".\installer_infoafter.txt"
  !insertmacro MUI_PAGE_FINISH 
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------

; The stuff to install
Section "Flash Multi"

  SectionIn RO
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Get the files
  File /r ".\flash-multi\bin\Release\bootloaders"
  File /r ".\flash-multi\bin\Release\drivers"
  File /r ".\flash-multi\bin\Release\tools"
  File ".\flash-multi\bin\Release\flash-multi.exe"
  File ".\flash-multi\bin\Release\flash-multi.exe.config"
  File ".\flash-multi\bin\Release\GPL.txt"
  File ".\flash-multi\bin\Release\license.txt"
  
  ; Write the uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\FlashMulti "Install_Dir" "$INSTDIR"

  ; Write the uninstall keys for Windows
  !define AddRemoveProgsReg "Software\Microsoft\Windows\CurrentVersion\Uninstall\FlashMulti"

  ${GetSize} "$INSTDIR" "/S=0K" $0 $1 $2
  IntFmt $0 "0x%08X" $0
  WriteRegDWORD HKLM "${AddRemoveProgsReg}" "EstimatedSize" "$0"

  WriteRegStr HKLM "${AddRemoveProgsReg}""Publisher" "Ben Lye"
  WriteRegStr HKLM "${AddRemoveProgsReg}""DisplayIcon" '"$INSTDIR\flash-multi.exe"'
  WriteRegStr HKLM "${AddRemoveProgsReg}""DisplayName" "Flash Multi ${VERSION}"
  WriteRegStr HKLM "${AddRemoveProgsReg}""DisplayVersion" "${VERSION}"
  WriteRegStr HKLM "${AddRemoveProgsReg}""URLInfoAbout" "https://github.com/benlye/flash-multi/"
  WriteRegStr HKLM "${AddRemoveProgsReg}""UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "${AddRemoveProgsReg}""NoModify" 1
  WriteRegDWORD HKLM "${AddRemoveProgsReg}""NoRepair" 1

SectionEnd

; Optional section (can be disabled by the user)
Section "Run the Maple USB driver installer"
  ; Install the drivers - https://github.com/pbatard/libwdi/blob/master/examples/wdi-simple.iss

  SetDetailsPrint textonly ; or both
  DetailPrint 'Installing Maple USB driver ...'
  SetDetailsPrint none   ; or listonly
  nsExec::Exec '"$INSTDIR\drivers\wdi-simple.exe" --vid 0x1EAF --pid 0x0003 --type 1 --name "Maple DFU" --dest "$TEMP\maple-dfu" --progressbar=$HWNDPARENT --timeout 120000"'

  SetDetailsPrint textonly ; or both
  DetailPrint 'Installing Maple Serial driver ...'
  SetDetailsPrint none   ; or listonly
  nsExec::Exec '"$INSTDIR\drivers\wdi-simple.exe" --vid 0x1EAF --pid 0x0004 --type 3 --name "Maple Serial" --dest "$TEMP\maple-serial" --progressbar=$HWNDPARENT --timeout 120000"'
  
SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\Flash Multi"
  CreateShortcut "$SMPROGRAMS\Flash Multi\Flash Multi.lnk" "$INSTDIR\flash-multi.exe" "" "$INSTDIR\flash-multi.exe" 0
  CreateShortcut "$SMPROGRAMS\Flash Multi\Run Maple Driver Installer.lnk" "$INSTDIR\drivers\install_drivers.bat"
  CreateShortcut "$SMPROGRAMS\Flash Multi\Uninstall Flash Multi.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  
SectionEnd

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\FlashMulti"
  DeleteRegKey HKLM SOFTWARE\FlashMulti

  ; Remove files
  Delete $INSTDIR\*.*

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\Flash Multi\*.*"

  ; Remove directories used
  RMDir "$SMPROGRAMS\Flash Multi"
  RMDir /r "$INSTDIR"

SectionEnd
