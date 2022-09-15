﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DynamicPatcher;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 4468)]
    public struct MapClass
    {
        private static IntPtr instance = new IntPtr(0x87F7E8);
        public static ref MapClass Instance { get => ref instance.Convert<MapClass>().Ref; }

        public const int MaxCells = 0x40000;

        public bool TryGetCellAt(CellStruct MapCoords, out Pointer<CellClass> pCell)
        {
            pCell = Pointer<CellClass>.Zero;

            int idx = GetCellIndex(MapCoords);
            if (idx >= 0 && idx < MaxCells)
            {
                pCell = Cells[idx];
            }

            return pCell != Pointer<CellClass>.Zero;
        }
        public bool TryGetCellAt(CoordStruct Crd, out Pointer<CellClass> pCell)
        {
            CellStruct cell = CellClass.Coord2Cell(Crd);
            return this.TryGetCellAt(cell, out pCell);
        }

        public unsafe Pointer<CellClass> GetCellAt(CoordStruct pCoords)
        {
            var func = (delegate* unmanaged[Thiscall]<ref MapClass, ref CoordStruct, IntPtr>)0x565730;
            return func(ref this, ref pCoords);
        }

        public static int GetCellIndex(CellStruct MapCoords)
        {
            return (MapCoords.Y << 9) + MapCoords.X;
        }

        // gets a coordinate in a random direction a fixed distance in leptons away from coords
        public static unsafe Pointer<CoordStruct> GetRandomCoordsNear(ref CoordStruct outBuffer, CoordStruct coords, int distance, bool center)
        {
            var func = (delegate* unmanaged[Thiscall]<int, ref CoordStruct, ref CoordStruct, int, Bool, IntPtr>)ASM.FastCallTransferStation;
            return func(0x49F420, ref outBuffer, ref coords, distance, center);
        }
        // gets a coordinate in a random direction a fixed distance in leptons away from coords
        public static unsafe CoordStruct GetRandomCoordsNear(ref CoordStruct coords, int distance, bool center)
        {
            CoordStruct outBuffer = default;
            GetRandomCoordsNear(ref outBuffer, coords, distance, center);
            return outBuffer;
        }

        // no fast call. unmanaged call will lead to StackOverflowException.
        //[UnmanagedFunctionPointer(CallingConvention.FastCall)]
        //public delegate DamageAreaResult DamageAreaFunction(in CoordStruct Coords, int Damage, /*Pointer<TechnoClass>*/IntPtr SourceObject,
        //    IntPtr WH, bool AffectsTiberium, IntPtr SourceHouse);
        //public static DamageAreaFunction DamageAreaDlg = Marshal.GetDelegateForFunctionPointer<DamageAreaFunction>(new IntPtr(0x489280));

        //public static unsafe DamageAreaResult DamageArea(in CoordStruct Coords, int Damage, Pointer<TechnoClass> SourceObject,
        //    Pointer<WarheadTypeClass> WH, bool AffectsTiberium, Pointer<HouseClass> SourceHouse)
        //{
        //    //var func = (delegate* unmanaged[Fastcall]<in CoordStruct, int, IntPtr, IntPtr, bool, IntPtr, DamageAreaResult>)0x489280;
        //    var func = (delegate* managed<in CoordStruct, int, Pointer<HouseClass>, bool, Pointer<WarheadTypeClass>, Pointer<TechnoClass>, DamageAreaResult>)0x489280;
        //    return func(in Coords, Damage, SourceHouse, AffectsTiberium, WH, SourceObject);
        //}

        //[UnmanagedFunctionPointer(CallingConvention.FastCall)]
        //public delegate void FlashbangWarheadAtFunction(int Damage, IntPtr WH, CoordStruct coords, bool Force = false, SpotlightFlags CLDisableFlags = SpotlightFlags.None);
        //public static FlashbangWarheadAtFunction FlashbangWarheadAt = Marshal.GetDelegateForFunctionPointer<FlashbangWarheadAtFunction>(new IntPtr(0x48A620));

        //public static unsafe void FlashbangWarheadAt(int Damage, Pointer<WarheadTypeClass> WH, CoordStruct coords, bool Force = false, SpotlightFlags CLDisableFlags = SpotlightFlags.None)
        //{
        //    //var func = (delegate* unmanaged[Fastcall]<int, IntPtr, CoordStruct, bool, SpotlightFlags, void>)0x48A620;
        //    var func = (delegate* managed<int, Pointer<WarheadTypeClass>, SpotlightFlags, bool, CoordStruct, void>)0x48A620;
        //    func(Damage, WH, CLDisableFlags, Force, coords);
        //}

        // get the damage a warhead causes to specific armor
        public static unsafe int GetTotalDamage(int Damage, Pointer<WarheadTypeClass> WH, Armor armor, int distance)
        {
            var func = (delegate* unmanaged[Thiscall]<int, int, IntPtr, Armor, int, int>)ASM.FastCallTransferStation;
            return func(0x489180, Damage, WH, armor, distance);
        }

        public static unsafe DamageAreaResult DamageArea(CoordStruct Coords, int Damage, Pointer<TechnoClass> SourceObject,
           Pointer<WarheadTypeClass> WH, bool AffectsTiberium, Pointer<HouseClass> SourceHouse)
        {
            var func = (delegate* unmanaged[Thiscall]<int, in CoordStruct, int, IntPtr, IntPtr, Bool, IntPtr, DamageAreaResult>)ASM.FastCallTransferStation;
            return func(0x489280, in Coords, Damage, SourceObject, WH, AffectsTiberium, SourceHouse);
        }

        /*
         * Picks the appropriate anim from WH's AnimList= based on damage dealt and land type (Conventional= )
         * so after DamageArea:
         * if(AnimTypeClass *damageAnimType = SelectDamageAnimation(...)) {
         * 	GameCreate<AnimClass>(damageAnimType, location);
         * }
         */
        public static unsafe Pointer<AnimTypeClass> SelectDamageAnimation(int Damage, Pointer<WarheadTypeClass> WH, LandType LandType, CoordStruct coords)
        {
            var func = (delegate* unmanaged[Thiscall]<int, int, IntPtr, LandType, in CoordStruct, IntPtr>)ASM.FastCallTransferStation;
            return func(0x48A4F0, Damage, WH, LandType, in coords);
        }

        public static unsafe void FlashbangWarheadAt(int Damage, Pointer<WarheadTypeClass> WH, CoordStruct coords, bool Force = false, SpotlightFlags CLDisableFlags = SpotlightFlags.None)
        {
            var func = (delegate* unmanaged[Thiscall]<int, int, IntPtr, CoordStruct, Bool, SpotlightFlags, void>)ASM.FastCallTransferStation;
            func(0x48A620, Damage, WH, coords, Force, CLDisableFlags);
        }

        public static CoordStruct Cell2Coord(CellStruct cell, int z = 0)
        {
            return new CoordStruct(cell.X * 256 + 128, cell.Y * 256 + 128, z);
        }

        public static CellStruct Coord2Cell(CoordStruct crd)
        {
            return new CellStruct(crd.X / 256, crd.Y / 256);
        }

        public unsafe CellStruct PickCellOnEdge(Edge Edge, CellStruct CurrentLocation, CellStruct Fallback,
            SpeedType SpeedType, bool ValidateReachability, MovementZone MovZone)
        {
            var func = (delegate* unmanaged[Thiscall]<ref MapClass, out CellStruct, Edge, ref CellStruct, ref CellStruct, SpeedType, Bool, MovementZone, IntPtr>)0x4AA440;
            func(ref this, out CellStruct tmp, Edge, ref CurrentLocation, ref Fallback, SpeedType, ValidateReachability, MovZone);
            return tmp;
        }

        // Find nearest spot
        public unsafe Pointer<CellStruct> Pathfinding_Find(ref CellStruct outBuffer, ref CellStruct position, SpeedType SpeedType, int a5, MovementZone MovementZone, bool alt, int SpaceSizeX, int SpaceSizeY, bool disallowOverlay, bool a11, bool requireBurrowable, bool allowBridge, ref CellStruct closeTo, bool a15, bool buildable)
        {
            var func = (delegate* unmanaged[Thiscall]<ref MapClass, ref CellStruct, ref CellStruct, SpeedType, int, MovementZone, Bool, int, int, Bool, Bool, Bool, Bool, ref CellStruct, Bool, Bool, IntPtr>)0x56DC20;
            return func(ref this, ref outBuffer, ref position, SpeedType, a5, MovementZone, alt, SpaceSizeX, SpaceSizeY, disallowOverlay, a11, requireBurrowable, allowBridge, ref closeTo, a15, buildable);
        }

        public unsafe CellStruct Pathfinding_Find(CellStruct position, SpeedType SpeedType, int a5, MovementZone MovementZone, bool alt, int SpaceSizeX, int SpaceSizeY, bool disallowOverlay, bool a11, bool requireBurrowable, bool allowBridge, CellStruct closeTo, bool a15, bool buildable)
        {
            CellStruct outBuffer = default;
            Pathfinding_Find(ref outBuffer, ref position, SpeedType, a5, MovementZone, alt, SpaceSizeX, SpaceSizeY, disallowOverlay, a11, requireBurrowable, allowBridge, ref closeTo, a15, buildable);
            return outBuffer;
        }

        public unsafe CellStruct Pathfinding_Find(ref CellStruct postion, SpeedType speedType, MovementZone movementZone, int extentX, int extentY, bool buildable)
        {
            int a5 = -1; // usually MapClass::CanLocationBeReached call. see how far we can get without it
            return Pathfinding_Find(postion, speedType, a5, movementZone, false, extentX, extentY, true, false, false, false, CellStruct.Empty, false, buildable);
        }

        [FieldOffset(312)] public DynamicVectorClass<Pointer<CellClass>> Cells;
        [FieldOffset(4444)] public DynamicVectorClass<CellStruct> TaggedCells;
    }
}
