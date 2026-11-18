@echo off
rem Registers the add-in with COM and SOLIDWORKS. Run from an elevated (Administrator) prompt.
setlocal
set CONFIG=%1
if "%CONFIG%"=="" set CONFIG=Debug
set DLL=%~dp0..\src\HelloSwAddin\bin\%CONFIG%\net48\HelloSwAddin.dll
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe" /codebase "%DLL%"
