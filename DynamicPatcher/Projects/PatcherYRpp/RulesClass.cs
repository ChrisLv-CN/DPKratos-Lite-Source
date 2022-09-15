using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PatcherYRpp
{
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct DifficultyStruct
    {
        [FieldOffset(0)] public double Firepower;
        [FieldOffset(8)] public double GroundSpeed;
        [FieldOffset(16)] public double AirSpeed;
        [FieldOffset(24)] public double Armor;
        [FieldOffset(32)] public double ROF;
        [FieldOffset(40)] public double Cost;
        [FieldOffset(48)] public double BuildTime;
        [FieldOffset(56)] public double RepairDelay;
        [FieldOffset(64)] public double BuildDelay;
        [FieldOffset(72)] public Bool BuildSlowdown;
        [FieldOffset(73)] public Bool DestroyWalls;
        [FieldOffset(74)] public Bool ContentScan;
    };

    [StructLayout(LayoutKind.Explicit, Size = 6336)]
    public struct RulesClass
    {
        private static IntPtr ppInstance = new IntPtr(0x8871E0);

        public static ref Pointer<RulesClass> Instance => ref ppInstance.Convert<Pointer<RulesClass>>().Ref;
        

        // call this instead of Init for the later files (gamemode, map)
        // reads the generic/list sections like [VehicleTypes] from pINI
        // doesn't actually load [MTNK] or other list contents' sections
        public unsafe void Read_File(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x668BF0;
            func(ref this, pINI);
        }

        public unsafe void Read_SpecialWeapons(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x668FB0;
            func(ref this, pINI);
        }

        public unsafe void Read_AudioVisual(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6691E0;
            func(ref this, pINI);
        }

        public unsafe void Read_CrateRules(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66B900;
            func(ref this, pINI);
        }

        public unsafe void Read_CombatDamage(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66BBB0;
            func(ref this, pINI);
        }

        public unsafe void Read_Radiation(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66CF70;
            func(ref this, pINI);
        }

        public unsafe void Read_ElevationModel(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66D150;
            func(ref this, pINI);
        }

        public unsafe void Read_WallModel(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66D1F0;
            func(ref this, pINI);
        }
        
        public unsafe void Read_Difficulty(Pointer<CCINIClass> pINI, ref DifficultyStruct difficultyStruct, AnsiString difficulty)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, ref DifficultyStruct, IntPtr, void>)0x66D270;
            func(ref this, pINI, ref difficultyStruct, difficulty);
        }

        public unsafe void Read_Colors(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66D3A0;
            func(ref this, pINI);
        }

        public unsafe void Read_ColorAdd(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66D480;
            func(ref this, pINI);
        }

        public unsafe void Read_General(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x66D530;
            func(ref this, pINI);
        }

        public unsafe void Read_MultiplayerDialogSettings(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x671EA0;
            func(ref this, pINI);
        }

        public unsafe void Read_Maximums(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672230;
            func(ref this, pINI);
        }

        public unsafe void Read_InfantryTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672280;
            func(ref this, pINI);
        }

        public unsafe void Read_Countries(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6722F0;
            func(ref this, pINI);
        }

        public unsafe void Read_VehicleTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672360;
            func(ref this, pINI);
        }

        public unsafe void Read_AircraftTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6723D0;
            func(ref this, pINI);
        }

        public unsafe void Read_Sides(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672440;
            func(ref this, pINI);
        }

        public unsafe void Read_SuperWeaponTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6725F0;
            func(ref this, pINI);
        }

        public unsafe void Read_BuildingTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672660;
            func(ref this, pINI);
        }

        public unsafe void Read_TerrainTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6726D0;
            func(ref this, pINI);
        }

        public unsafe void Read_Teams_obsolete(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672740;
            func(ref this, pINI);
        }

        public unsafe void Read_SmudgeTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6727D0;
            func(ref this, pINI);
        }

        public unsafe void Read_OverlayTypes(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672840;
            func(ref this, pINI);
        }

        public unsafe void Read_Animations(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6728B0;
            func(ref this, pINI);
        }

        public unsafe void Read_VoxelAnims(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672920;
            func(ref this, pINI);
        }

        public unsafe void Read_Warheads(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672990;
            func(ref this, pINI);
        }

        public unsafe void Read_Particles(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672A00;
            func(ref this, pINI);
        }

        public unsafe void Read_ParticleSystems(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672A70;
            func(ref this, pINI);
        }

        public unsafe void Read_AI(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x672AE0;
            func(ref this, pINI);
        }

        public unsafe void Read_Powerups(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x673E80;
            func(ref this, pINI);
        }

        public unsafe void Read_LandCharacteristics(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x674000;
            func(ref this, pINI);
        }

        public unsafe void Read_IQ(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x674240;
            func(ref this, pINI);
        }

        public unsafe void Read_JumpjetControls(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x6743D0;
            func(ref this, pINI);
        }

        public unsafe void Read_Difficulties(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x674500;
            func(ref this, pINI);
        }

        public unsafe void Read_Movies(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x674550;
            func(ref this, pINI);
        }

        public unsafe void Read_AdvancedCommandBar(Pointer<CCINIClass> pINI, bool isMultiplayer)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, Bool, void>)0x674650;
            func(ref this, pINI, isMultiplayer);
        }

        public unsafe void Read_Tiberiums(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x721D10;
            func(ref this, pINI);
        }
        /// <summary>
        /// invoke AbstractTypeClass.LoadFromINI for some classes and load MissionControlClass.
        /// the classes to LoadFromINI are below:
        /// HouseTypeClass, SuperWeaponTypeClass, AnimTypeClass, BuildingTypeClass,
        /// AircraftTypeClass, UnitTypeClass, InfantryTypeClass, WeaponTypeClass,
        /// BulletTypeClass, WarheadTypeClass, WeaponTypeClass, TerrainTypeClass,
        /// SmudgeTypeClass, OverlayTypeClass, ParticleTypeClass, ParticleSystemTypeClass,
        /// VoxelAnimTypeClass
        /// </summary>
        /// <param name="pINI"></param>
        public unsafe void Read_Types(Pointer<CCINIClass> pINI)
        {
            var func = (delegate* unmanaged[Thiscall]<ref RulesClass, IntPtr, void>)0x679A10;
            func(ref this, pINI);
        }

        public static RulesClass Global()
        {
            return Instance.Ref;
        }

        [FieldOffset(1640)] public double VeteranRatio;
        [FieldOffset(1648)] public double VeteranCombat;
        [FieldOffset(1656)] public double VeteranSpeed;
        [FieldOffset(1664)] public double VeteranSight;
        [FieldOffset(1672)] public double VeteranArmor;
        [FieldOffset(1680)] public double VeteranROF;
        [FieldOffset(1688)] public double VeteranCap;
        [FieldOffset(3008)] public DynamicVectorClass<AnimTypeClass> SplashList;
        [FieldOffset(3972)] public Pointer<WarheadTypeClass> FlameDamage;
        [FieldOffset(3976)] public Pointer<WarheadTypeClass> FlameDamage2;
        [FieldOffset(3980)] public Pointer<WarheadTypeClass> NukeWarhead;
        [FieldOffset(3984)] public Pointer<BulletTypeClass> NukeProjectile;
        [FieldOffset(3988)] public Pointer<BulletTypeClass> NukeDown;
        [FieldOffset(4008)] public Pointer<WarheadTypeClass> C4Warhead;
        [FieldOffset(4012)] public Pointer<WarheadTypeClass> CrushWarhead;
        [FieldOffset(5432)] public DifficultyStruct Easy;
        [FieldOffset(5512)] public DifficultyStruct Normal;
        [FieldOffset(5592)] public DifficultyStruct Difficult;
        [FieldOffset(5816)] public int Gravity;
        [FieldOffset(5832)] public int MaxDamage;
        [FieldOffset(5940)] public int BallisticScatter; // value in *256
        [FieldOffset(6148)] public int RadDurationMultiple;
        [FieldOffset(6152)] public int RadApplicationDelay;
        [FieldOffset(6156)] public int RadLevelMax;
        [FieldOffset(6160)] public int RadLevelDelay;
        [FieldOffset(6164)] public int RadLightDelay;
        [FieldOffset(6168)] public double RadLevelFactor;
        [FieldOffset(6176)] public double RadLightFactor;
        [FieldOffset(6184)] public double RadTintFactor;
        [FieldOffset(6192)] public ColorStruct RadColor;
        [FieldOffset(6196)] public Pointer<WarheadTypeClass> RadSiteWarhead;
        [FieldOffset(6246)] public ColorStruct ChronoBeamColor;
        [FieldOffset(6249)] public ColorStruct MagnaBeamColor;
        [FieldOffset(6260)] public ColorStruct colorAdd_first;
        public Pointer<ColorStruct> ColorAdd => Pointer<ColorStruct>.AsPointer(ref colorAdd_first);
        [FieldOffset(6308)] public int laserTargetColor;
        public static ColorStruct LaserTargetColor => Instance.Ref.ColorAdd[Instance.Ref.laserTargetColor];
        [FieldOffset(6312)] public int ironCurtainColor;
        public static ColorStruct IronCurtainColor => Instance.Ref.ColorAdd[Instance.Ref.ironCurtainColor];
        [FieldOffset(6316)] public int berserkColor;
        public static ColorStruct BerserkColor => Instance.Ref.ColorAdd[Instance.Ref.berserkColor];
        [FieldOffset(6320)] public int forceShieldColor;
        public static ColorStruct ForceShieldColor => Instance.Ref.ColorAdd[Instance.Ref.forceShieldColor];

    }
}
