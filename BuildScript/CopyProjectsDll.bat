@echo off
chcp 65001
cd %~dp0
echo f|xcopy /y "..\DynamicPatcher\Projects\PatcherYRpp\bin\Debug\PatcherYRpp.dll" "output\PatcherYRpp.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\PatcherYRpp.Utilities\bin\Debug\PatcherYRpp.Utilities.dll" "output\PatcherYRpp.Utilities.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\ScriptUniversal\bin\Debug\ScriptUniversal.dll" "output\ScriptUniversal.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\Extension\bin\Debug\Extension.dll" "output\Extension.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\Extension.Ext\bin\Debug\Extension.Ext.dll" "output\Extension.Ext.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\Extension.FX\bin\Debug\Extension.FX.dll" "output\Extension.FX.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\Extension.Script\bin\Debug\Extension.Script.dll" "output\Extension.Script.dll"
echo f|xcopy /y "..\DynamicPatcher\Projects\Extension.Kratos\bin\Debug\Extension.Kratos.dll" "output\Extension.Kratos.dll"
echo f|xcopy /y "..\DynamicPatcher\Kratos*.i*" "output\Kratos*.i*"

