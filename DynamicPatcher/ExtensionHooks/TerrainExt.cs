
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;

namespace ExtensionHooks
{
    public class TerrainExtHooks
    {

        [Hook(HookType.AresHook, Address = 0x71C950, Size = 6)]
        public static unsafe UInt32 TerrainClass_Remove_PlayDestroyAnim(REGISTERS* R)
        {
            try
            {
                Pointer<TerrainClass> pTerrain = (IntPtr)R->ESI;
                // Logger.Log($"{Game.CurrentFrame} [{pTerrain.Ref.Type.Ref.Base.Base.ID}]{pTerrain} is dead");
                TerrainDestroyAnim.PlayDestroyAnim(pTerrain);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

    }
}

