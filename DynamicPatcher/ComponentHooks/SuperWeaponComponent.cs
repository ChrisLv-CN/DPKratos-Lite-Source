using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;
using Extension.Ext;
using Extension.Script;
using PatcherYRpp;

namespace ComponentHooks
{
    public class SuperWeaponComponentHooks
    {
        [Hook(HookType.AresHook, Address = 0x6CC390, Size = 6)]
        public static unsafe UInt32 SuperClass_Launch_Components(REGISTERS* R)
        {
            try
            {
                Pointer<SuperClass> pSuper = (IntPtr)R->ECX;
                var pCell = R->Stack<Pointer<CellStruct>>(0x4);
                var isPlayer = R->Stack<bool>(0x8);

                SuperWeaponExt ext = SuperWeaponExt.ExtMap.Find(pSuper);
                ext.GameObject.Foreach(c => (c as ISuperWeaponScriptable)?.OnLaunch(pCell.Data, isPlayer));

                return 0;
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
                return 0;
            }
        }
    }
}
