using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct SuperClass : IOwnAbstractType<SuperWeaponTypeClass>
    {
        Pointer<SuperWeaponTypeClass> IOwnAbstractType<SuperWeaponTypeClass>.OwnType => Type;
        Pointer<AbstractTypeClass> IOwnAbstractType.AbstractType => Type.Convert<AbstractTypeClass>();

        public static readonly IntPtr ArrayPointer = new IntPtr(0xA83CB8);
        public static ref DynamicVectorClass<Pointer<SuperClass>> Array => ref DynamicVectorClass<Pointer<SuperClass>>.GetDynamicVector(ArrayPointer);

        public static readonly IntPtr ShowTimersPointer = new IntPtr(0xA83D50);
        public static ref DynamicVectorClass<Pointer<SuperClass>> ShowTimers => ref DynamicVectorClass<Pointer<SuperClass>>.GetDynamicVector(ArrayPointer);


        // invoked when sabotaged or SuperWeaponReset(Map) executed
        public unsafe void Reset()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, void>)0x6CE0B0;
            func(ref this);
        }

        public unsafe bool SetOnHold(bool onHold)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, Bool>)0x6CB4D0;
            return func(ref this, onHold);
        }

        public unsafe bool Grant(bool oneTime, bool announce, bool onHold)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, Bool, Bool, Bool>)0x6CB560;
            return func(ref this, oneTime, announce, onHold);
        }

        // true if this was ->Granted
        public unsafe bool Lose()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool>)0x6CB7B0;
            return func(ref this);
        }

        public bool IsPowered()
        {
            return Type.Ref.IsPowered;
        }

        public unsafe void Launch(CellStruct cell, bool isPlayer)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, ref CellStruct, Bool, void>)0x6CC390;
            func(ref this, ref cell, isPlayer);
        }

        public unsafe bool CanFire()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool>)0x6CC360;
            return func(ref this);
        }

        public unsafe void SetReadiness(bool ready)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, void>)0x6CB820;
            func(ref this, ready);
        }

        public unsafe byte StopPreclickAnim(bool isPlayer)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, byte>)0x6CB830;
            return func(ref this, isPlayer);
        }

        public unsafe byte ClickFire(bool isPlayer, CellStruct cell)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, ref CellStruct, byte>)0x6CB920;
            return func(ref this, isPlayer, ref cell);
        }

        // true if the charge has changed (charge overlay on the cameo)
        // triggers the EVA Announcement if it's ready
        public unsafe bool HasChargeProgressed(bool isPlayer)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool, Bool>)0x6CBCA0;
            return func(ref this, isPlayer);
        }

        public unsafe int GetCameoChargeState()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, int>)0x6CBEE0;
            return func(ref this);
        }

        public unsafe void SetCharge(int percentage)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, int, void>)0x6CC1E0;
            func(ref this, percentage);
        }

        public unsafe int GetRechargeTime()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, int>)0x6CC260;
            return func(ref this);
        }

        public unsafe void SetRechargeTime(int time)
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, int, void>)0x6CC280;
            func(ref this, time);
        }

        public unsafe void ResetRechargeTime()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, void>)0x6CC290;
            func(ref this);
        }

        public unsafe string NameReadiness()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, UniStringPointer>)0x6CC2B0;
            return func(ref this);
        }

        public unsafe bool ShouldDrawProgress()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool>)0x6CDE90;
            return func(ref this);
        }

        public unsafe bool ShouldFlashTab()
        {
            var func = (delegate* unmanaged[Thiscall]<ref SuperClass, Bool>)0x6CE1A0;
            return func(ref this);
        }


        [FieldOffset(0)] public AbstractClass Base;

        [FieldOffset(36)] public int CustomChargeTime;
        [FieldOffset(40)] public Pointer<SuperWeaponTypeClass> Type;
        [FieldOffset(44)] public IntPtr owner;
        public Pointer<HouseClass> Owner { get => owner; set => owner = value; }
        [FieldOffset(48)] public TimerStruct RechargeTimer;
        [FieldOffset(60)] public UniStringPointer UIName;
        [FieldOffset(64)] public Bool BlinkState;
        [FieldOffset(72)] public long BlinkTimer;
        [FieldOffset(80)] public int SpecialSoundDuration; // see 0x6CD14F
        [FieldOffset(84)] public CoordStruct SpecialSoundLocation;
        [FieldOffset(96)] public Bool CanHold;          // 0x60
        [FieldOffset(98)] public CellStruct ChronoMapCoords;  // 0x62
        [FieldOffset(104)] private IntPtr animation;                // 0x68
        public Pointer<AnimClass> Animation { get => animation; set => animation = value; }
        [FieldOffset(108)] public Bool AnimationGotInvalid;
        [FieldOffset(109)] public Bool Granted;
        [FieldOffset(110)] public Bool OneTime; // remove this SW when it has been fired once
        [FieldOffset(111)] public Bool IsCharged;
        [FieldOffset(112)] public Bool IsOnHold;
        [FieldOffset(116)] public int ReadinessFrame; // when did it become ready?
        [FieldOffset(120)] public int CameoChargeState;
        [FieldOffset(124)] public ChargeDrainState ChargeDrainState;
    }
}
