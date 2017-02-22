@echo off

REM eecs-494-unity-canvas-manual.bat Version 2.1

SET DP0=%~dp0%
SET TURNIN=%1
SET UNITY=C:\Program Files\Unity\Editor\Unity.exe
SET ZIP=C:\Program Files\7-Zip\7z.exe

SET ZIPFILE=%DP0%\%TURNIN%-private.7z
SET APP=%DP0%\%TURNIN%.app
SET EXE=%DP0%\%TURNIN%.exe
SET EXEDATA=%DP0%\%TURNIN%_Data
SET TEMPDIR=%TEMP%
SET REPODIR=%TEMPDIR%\%TURNIN%_Repo
SET REPOLINK=%TEMPDIR%\%TURNIN%_Link
SET EL=0

IF "%1" == "" (

ECHO(
ECHO Usage: eecs-494-unity-canvas-manual.bat TURNIN_NAME
ECHO(

EXIT /B 1
)

ECHO(

IF EXIST "%ZIPFILE%" DEL /Q "%ZIPFILE%"
IF EXIST "%REPOLINK%" RMDIR /S /Q "%REPOLINK%"
IF NOT EXIST "%APP%" (
  ECHO %APP% not found.
  SET EL=1
)
IF NOT EXIST "%EXE%" (
  ECHO %EXE% not found.
  SET EL=1
)
IF NOT EXIST "%EXEDATA%" (
  ECHO %EXEDATA% not found.
  SET EL=1
)

IF NOT %EL% == 0 (
  ECHO Build missing.
) ELSE (
  IF EXIST "%REPODIR%" RMDIR /S /Q "%REPODIR%"

  cd "%TEMPDIR%"

  ECHO MKLINK /J "%REPOLINK%" "%DP0%"
  MKLINK /J "%REPOLINK%" "%DP0%"
  
  ECHO git clone "%REPOLINK%" "%REPODIR%"
  git clone "%REPOLINK%" "%REPODIR%"
  IF ERRORLEVEL 1 (
    SET EL=1

    CD "%DP0%"
  ) ELSE (
    IF EXIST "%REPODIR%\.git" RMDIR /S /Q "%REPODIR%\.git"

    CD "%DP0%"

    IF %EL%==0 (
      ECHO "%ZIP%" a "%ZIPFILE%" "%APP%" "%EXE%" "%EXEDATA%" "%REPODIR%"
      "%ZIP%" a "%ZIPFILE%" "%APP%" "%EXE%" "%EXEDATA%" "%REPODIR%"
      IF ERRORLEVEL 1 SET EL=1
    )
  )

  IF EXIST "%REPODIR%" RMDIR /S /Q "%REPODIR%"
  IF EXIST "%REPOLINK%" RMDIR /S /Q "%REPOLINK%"
)

ECHO(

IF %EL%==0 (
  CertUtil -hashfile "%ZIPFILE%" sha512
  PowerShell Get-FileHash "\"%ZIPFILE%\"" -Algorithm SHA256
) ELSE (
  ECHO Something went wrong. See above for details.
)

EXIT /B
