using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using PatcherYRpp;

namespace Extension.EventSystems
{
    public class AnnounceExpiredPointerEvent : EventBase
    {
        public override string Name => "AnnounceExpiredPointer";
        public override string Description => "Raised when an AbstractClass pointer expired";
    }

    public class AnnounceExpiredPointerEventArgs : EventArgs
    {
        public AnnounceExpiredPointerEventArgs(Pointer<AbstractClass> expiredPointer, bool removed)
        {
            ExpiredPointer = expiredPointer;
            Removed = removed;
        }

        public Pointer<AbstractClass> ExpiredPointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }

        public bool Removed
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            protected set;
        }
    }

    public class PointerExpireEventSystem : EventSystem
    {
        public PointerExpireEventSystem()
        {
            AnnounceExpiredPointerEvent = new AnnounceExpiredPointerEvent();
        }

        public AnnounceExpiredPointerEvent AnnounceExpiredPointerEvent { get; }
    }
}
