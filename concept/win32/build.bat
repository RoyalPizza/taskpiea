@echo off
setlocal

set PLATFORM=x64
set CONFIGURATION=Debug
set OUTPUT_DIR=bin\%PLATFORM%\%CONFIGURATION%
set OUTPUT_EXE=%OUTPUT_DIR%\taskpiea.exe
set OBJ_DIR=obj\%PLATFORM%\%CONFIGURATION%

REM Find Visual Studio Build Tools
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat" (
    set "VCVARS=C:\Program Files\Microsoft Visual Studio\2022\Community\VC\Auxiliary\Build\vcvars64.bat"
    set MSVC_COMPILE=true
) else if exist "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" (
    set "VCVARS=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat"
    set MSVC_COMPILE=true
)

if not defined MSVC_COMPILE (
    echo Visual Studio Build Tools not found!
    pause
    exit /b 1
)

call "%VCVARS%" >nul

REM Clean output folders
if exist %OBJ_DIR% (
    del /Q %OBJ_DIR%\*.*
)
if exist %OUTPUT_DIR% (
    del /Q %OUTPUT_DIR%\*.*
)

REM Create directories if they don't exist
if not exist %OBJ_DIR% mkdir %OBJ_DIR%
if not exist %OUTPUT_DIR% mkdir %OUTPUT_DIR%

REM Compile with debug info
cl /c /EHsc /Zi /MDd /Od /W4 /Fo:%OBJ_DIR%\ /Fd:%OBJ_DIR%\taskpiea_obj.pdb main.cpp || exit /b 1

REM Link with debug info, subsystem, and gdi32.lib
link /OUT:%OUTPUT_EXE% /DEBUG:FULL /PDB:%OUTPUT_DIR%\taskpiea.pdb /SUBSYSTEM:WINDOWS /MACHINE:X64 %OBJ_DIR%\*.obj user32.lib comctl32.lib gdi32.lib || exit /b 1

if %ERRORLEVEL% == 0 (
    echo Compilation and linking successful!
) else (
    echo Build failed!
)

endlocal
exit /b 0