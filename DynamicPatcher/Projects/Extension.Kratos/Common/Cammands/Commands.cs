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
    public class Commands
    {
        // #define CATEGORY_TEAM StringTable::LoadString("TXT_TEAM")
        // #define CATEGORY_INTERFACE StringTable::LoadString("TXT_INTERFACE")
        // #define CATEGORY_TAUNT StringTable::LoadString("TXT_TAUNT")
        // #define CATEGORY_SELECTION StringTable::LoadString("TXT_SELECTION")
        // #define CATEGORY_CONTROL StringTable::LoadString("TXT_CONTROL")
        // #define CATEGORY_DEBUG L"Debug"
        // #define CATEGORY_GUIDEBUG StringTable::LoadString("GUI:Debug")
        // #define CATEGORY_DEVELOPMENT StringTable::LoadString("TXT_DEVELOPMENT")

        // private static UniStringPointer category_TEAM = "TXT_TEAM";
        // public static UniStringPointer CATEGORY_TEAM
        // {
        //     get
        //     {
        //         return category_TEAM;
        //     }
        // }

        public static UniString CATEGORY_KRATOS = new UniString("Kratos");

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void DTORFunc(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate AnsiStringPointer GetNameFunc(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate UniStringPointer GetUINameFunc(IntPtr pThis);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate Bool CheckInputFunc(IntPtr pThis, WWKey input);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate void ExecuteFunc(IntPtr pThis, WWKey input);

        public static void MakeCommand<T>()
        {
            Pointer<T> item = YRMemory.Create<T>();
            CommandClass.ABSTRACTTYPE_ARRAY.Array.AddItem(item.Convert<CommandClass>());
        }
    }
}
