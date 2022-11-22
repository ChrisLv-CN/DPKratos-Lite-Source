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
    // 模板
    public struct DummyCommand
    {
        public static AnsiString Name = new AnsiString("Dummy");
        public static UniString UIName = new UniString("DummyUIName");
        public static UniString UIDescription = new UniString("DummyUIDescription");

        public static Commands.ExecuteFunc Execute = ExecuteProxy;
        public static void ExecuteProxy(IntPtr pThis, WWKey input)
        {
            // do something
            Logger.Log($"{Game.CurrentFrame} Dummy ExecuteProxy {input}");
        }

        public static Commands.DTORFunc DTOR = DTORProxy;
        public static void DTORProxy(IntPtr pThis) { }

        public static Commands.GetNameFunc GetName = GetNameProxy;
        public static AnsiStringPointer GetNameProxy(IntPtr pThis)
        {
            return Name;
        }

        public static Commands.GetUINameFunc GetUIName = GetUINameProxy;
        public static UniStringPointer GetUINameProxy(IntPtr pThis)
        {
            return UIName;
        }

        public static Commands.GetUINameFunc GetUICategory = GetUICategoryProxy;
        public static UniStringPointer GetUICategoryProxy(IntPtr pThis)
        {
            return Commands.CATEGORY_KRATOS;
        }

        public static Commands.GetUINameFunc GetUIDescription = GetUIDescriptionProxy;
        public static UniStringPointer GetUIDescriptionProxy(IntPtr pThis)
        {
            return UIDescription;
        }

        public static Commands.CheckInputFunc PreventCombinationOverride = PreventCombinationOverrideProxy;
        // Do we need to check extra value like SHIFT?
        // If this value is true, the game won't process
        // Combination keys written here
        // e.g. To ignore SHIFT + this key
        // return eInput & WWKey::Shift;
        public static Bool PreventCombinationOverrideProxy(IntPtr pThis, WWKey input)
        {
            return false;
        }

        public static Commands.CheckInputFunc ExtraTriggerCondition = ExtraTriggerConditionProxy;
        // Only with this key set to true will the game call the Execute
        public static Bool ExtraTriggerConditionProxy(IntPtr pThis, WWKey input)
        {
            return !Convert.ToBoolean(input & WWKey.Release);
        }

        public static Commands.CheckInputFunc CheckLoop55E020 = CheckLoop55E020Proxy;
        // Stupid loop, I don't know what's it used for
        public static Bool CheckLoop55E020Proxy(IntPtr pThis, WWKey input)
        {
            return false;
        }

        public static unsafe void Constructor(Pointer<DummyCommand> pThis)
        {
            Pointer<IntPtr> VTable = Marshal.AllocHGlobal(sizeof(IntPtr) * 9);
            VTable[0] = Marshal.GetFunctionPointerForDelegate(DTOR);
            VTable[1] = Marshal.GetFunctionPointerForDelegate(GetName);
            VTable[2] = Marshal.GetFunctionPointerForDelegate(GetUIName);
            VTable[3] = Marshal.GetFunctionPointerForDelegate(GetUICategory);
            VTable[4] = Marshal.GetFunctionPointerForDelegate(GetUIDescription);
            VTable[5] = Marshal.GetFunctionPointerForDelegate(PreventCombinationOverride);
            VTable[6] = Marshal.GetFunctionPointerForDelegate(ExtraTriggerCondition);
            VTable[7] = Marshal.GetFunctionPointerForDelegate(CheckLoop55E020);
            VTable[8] = Marshal.GetFunctionPointerForDelegate(Execute);

            Pointer<Pointer<IntPtr>> pVfptr = (IntPtr)pThis;
            pVfptr.Data = VTable;
        }
    }


}
