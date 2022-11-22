using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public struct AddAECommand
    {
        public static AnsiString Name = new AnsiString("AddAE");
        public static UniString UIName = new UniString("AddAEUIName");
        public static UniString UIDescription = new UniString("AddAEUIDescription");

        public static void DTORProxy(IntPtr pThis) { }
        public static AnsiStringPointer GetNameProxy(IntPtr pThis)
        {
            Logger.Log("AddAE GetNameProxy");
            return Name;
        }
        public static UniStringPointer GetUINameProxy(IntPtr pThis)
        {
            Logger.Log("AddAE GetUINameProxy");
            return UIName;
        }
        public static UniStringPointer GetUICategoryProxy(IntPtr pThis)
        {
            Logger.Log("AddAE GetUICategoryProxy");
            return Commands.CATEGORY_KRATOS;
        }
        public static UniStringPointer GetUIDescriptionProxy(IntPtr pThis)
        {
            Logger.Log("AddAE GetUIDescriptionProxy");
            return UIDescription;
        }
        // Do we need to check extra value like SHIFT?
        // If this value is true, the game won't process
        // Combination keys written here
        // e.g. To ignore SHIFT + this key
        // return eInput & WWKey::Shift;
        public static Bool PreventCombinationOverrideProxy(IntPtr pThis, WWKey input)
        {
            Logger.Log($"{Game.CurrentFrame} AddAE PreventCombinationOverrideProxy {input}");
            return false;
        }
        // Only with this key set to true will the game call the Execute
        public static Bool ExtraTriggerConditionProxy(IntPtr pThis, WWKey input)
        {
            Logger.Log($"{Game.CurrentFrame} AddAE ExtraTriggerConditionProxy {input}");
            // return !(input & WWKey.Release);
            return false;
        }
        // Stupid loop, I don't know what's it used for
        public static Bool CheckLoop55E020Proxy(IntPtr pThis, WWKey input)
        {
            Logger.Log($"{Game.CurrentFrame} AddAE CheckLoop55E020Proxy {input}");
            return false;
        }
        public static void ExecuteProxy(IntPtr pThis, WWKey input)
        {
            // do something
            Logger.Log($"{Game.CurrentFrame} AddAE ExecuteProxy {input}");
        }

        public static unsafe void Constructor(Pointer<AddAECommand> pThis)
        {
            Pointer<IntPtr> VTable = Marshal.AllocHGlobal(sizeof(IntPtr) * 9);
            VTable[0] = Marshal.GetFunctionPointerForDelegate((Commands.DTORFunc)DTORProxy);
            VTable[1] = Marshal.GetFunctionPointerForDelegate((Commands.GetNameFunc)GetNameProxy);
            VTable[2] = Marshal.GetFunctionPointerForDelegate((Commands.GetUINameFunc)GetUINameProxy);
            VTable[3] = Marshal.GetFunctionPointerForDelegate((Commands.GetUINameFunc)GetUICategoryProxy);
            VTable[4] = Marshal.GetFunctionPointerForDelegate((Commands.GetUINameFunc)GetUIDescriptionProxy);
            VTable[5] = Marshal.GetFunctionPointerForDelegate((Commands.CheckInputFunc)PreventCombinationOverrideProxy);
            VTable[6] = Marshal.GetFunctionPointerForDelegate((Commands.CheckInputFunc)ExtraTriggerConditionProxy);
            VTable[7] = Marshal.GetFunctionPointerForDelegate((Commands.CheckInputFunc)CheckLoop55E020Proxy);
            VTable[8] = Marshal.GetFunctionPointerForDelegate((Commands.ExecuteFunc)ExecuteProxy);

            Pointer<Pointer<IntPtr>> pVfptr = (IntPtr)pThis;
            pVfptr.Data = VTable;

            Logger.Log("AddAE created");
        }

        // public CommandClass Base;
    }


}
