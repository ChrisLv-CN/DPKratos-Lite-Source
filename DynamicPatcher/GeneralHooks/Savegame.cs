using DynamicPatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;
using System.Runtime.InteropServices.ComTypes;
using PatcherYRpp;

namespace GeneralHooks
{
    public class Savegame
    {
        [Hook(HookType.AresHook, Address = 0x67CEF0, Size = 6)]
        public static unsafe UInt32 SaveGame_Start(REGISTERS* R)
        {
            string fileName = (AnsiStringPointer)(IntPtr)R->ECX;
            string scenarioDescription = (AnsiStringPointer)(IntPtr)R->EDX;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(fileName, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67D2F1, Size = 0x6)]
        public static unsafe UInt32 SaveGame_End(REGISTERS* R)
        {
            string fileName = (AnsiStringPointer)(IntPtr)R->EDI;
            string scenarioDescription = (AnsiStringPointer)(IntPtr)R->ESI;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(fileName, false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67D300, Size = 5)]
        public static unsafe UInt32 SaveGameInStream_Start(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ECX;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(stream, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E42E, Size = 0xD)]
        public static unsafe UInt32 SaveGameInStream_End(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ESI;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.SaveGameEvent,
                new SaveGameEventArgs(stream, false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E440, Size = 6)]
        public static unsafe UInt32 LoadGame_Start(REGISTERS* R)
        {
            string fileName = (AnsiStringPointer)(IntPtr)R->ECX;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(fileName, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E720, Size = 6)]
        public static unsafe UInt32 LoadGame_End(REGISTERS* R)
        {
            string fileName = (AnsiStringPointer)(IntPtr)R->ESI;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(fileName, false));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67E730, Size = 5)]
        public static unsafe UInt32 LoadGameInStream_Start(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ECX;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(stream, true));

            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x67F7C8, Size = 5)]
        public static unsafe UInt32 LoadGameInStream_End(REGISTERS* R)
        {
            var pStm = (Pointer<IStream>)R->ESI;
            IStream stream = Marshal.GetObjectForIUnknown(pStm) as IStream;

            EventSystem.SaveGame.Broadcast(
                EventSystem.SaveGame.LoadGameEvent,
                new LoadGameEventArgs(stream, false));

            return 0;
        }
    }
}
