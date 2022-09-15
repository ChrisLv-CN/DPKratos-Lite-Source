using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Extension.EventSystems;

namespace GeneralHooks
{
    public class PointerExpire
    {
        class BufferedAnnounceExpiredPointerEventArgs : AnnounceExpiredPointerEventArgs
        {
            public BufferedAnnounceExpiredPointerEventArgs() : base(Pointer<AbstractClass>.Zero, false)
            {
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetExpiredPointer(Pointer<AbstractClass> pExpired)
            {
                ExpiredPointer = pExpired;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SetRemoved(bool removed)
            {
                Removed = removed;
            }
        }
        private static BufferedAnnounceExpiredPointerEventArgs _bufferedEventArgs = new();

        [Hook(HookType.AresHook, Address = 0x7258D0, Size = 6)]
        public static unsafe UInt32 AnnounceExpiredPointer(REGISTERS* R)
        {
            var pAbstract = (Pointer<AbstractClass>)R->ECX;
            bool removed = (Bool)R->DL;

            _bufferedEventArgs.SetExpiredPointer(pAbstract);
            _bufferedEventArgs.SetRemoved(removed);

            EventSystem.PointerExpire.Broadcast(EventSystem.PointerExpire.AnnounceExpiredPointerEvent, _bufferedEventArgs);

            return 0;
        }


    }
}
