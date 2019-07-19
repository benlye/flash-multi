@echo off

echo Installing Maple DFU driver...
"%~dp0wdi-simple" --vid 0x1EAF --pid 0x0003 --type 2 --name "Maple DFU" --dest "%~dp0maple-dfu" -b
echo.

echo Installing Maple Serial driver...
"%~dp0wdi-simple" --vid 0x1EAF --pid 0x0004 --type 3 --name "Maple Serial" --dest "%~dp0maple-serial" -b
echo.
