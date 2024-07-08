@ECHO OFF
SETLOCAL ENABLEDELAYEDEXPANSION

SET OBJ=RALLYDX.exe

SET EXE="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
SET REF="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF"

SET TYP=/t:winexe
SET ICO=-win32icon:app.ico

SET DLL=/r:%REF%\PresentationCore.dll
SET DLL=%DLL% /r:%REF%\PresentationFramework.dll
SET DLL=%DLL% /r:%REF%\WindowsBase.dll

SET OUT=/out:%OBJ%

TASKKILL /F /IM %OBJ%

SET RES=
FOR %%F IN (resources/*.*) DO (
    SET RES=!RES! /res:resources\%%F
)

%EXE% %ICO% %TYP% %OUT% %DLL% /o src\*.cs

IF ERRORLEVEL 1 (
  ECHO  ************************
  ECHO  *** コンパイルエラー ***
  ECHO  ************************
  PAUSE
)
REM PAUSE