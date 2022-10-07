@echo off
chcp 65001
cd %~dp0
rd /S /Q output
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\PatcherYRpp\bin\Debug\PatcherYRpp.dll" "output\PatcherYRpp.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\PatcherYRpp\bin\Debug\PatcherYRpp.pdb" "output\PatcherYRpp.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\PatcherYRpp.Utilities\bin\Debug\PatcherYRpp.Utilities.dll" "output\PatcherYRpp.Utilities.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\PatcherYRpp.Utilities\bin\Debug\PatcherYRpp.Utilities.pdb" "output\PatcherYRpp.Utilities.pdb"
::echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\ScriptUniversal\bin\Debug\ScriptUniversal.dll" "output\ScriptUniversal.dll"
::echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\ScriptUniversal\bin\Debug\ScriptUniversal.pdb" "output\ScriptUniversal.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension\bin\Debug\Extension.dll" "output\Extension.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension\bin\Debug\Extension.pdb" "output\Extension.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Ext\bin\Debug\Extension.Ext.dll" "output\Extension.Ext.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Ext\bin\Debug\Extension.Ext.pdb" "output\Extension.Ext.pdb"
::echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.FX\bin\Debug\Extension.FX.dll" "output\Extension.FX.dll"
::echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.FX\bin\Debug\Extension.FX.pdb" "output\Extension.FX.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Script\bin\Debug\Extension.Script.dll" "output\Extension.Script.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Script\bin\Debug\Extension.Script.pdb" "output\Extension.Script.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Kratos\bin\Debug\Extension.Kratos.dll" "output\Extension.Kratos.dll"
echo f|xcopy /i /D /y "..\DynamicPatcher\Projects\Extension.Kratos\bin\Debug\Extension.Kratos.pdb" "output\Extension.Kratos.pdb"
echo f|xcopy /i /D /y "..\DynamicPatcher\Kratos*.i*" "output\Kratos*.i*"

