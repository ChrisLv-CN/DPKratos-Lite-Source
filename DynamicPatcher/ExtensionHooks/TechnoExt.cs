
using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Utilities;
using Extension.Script;

namespace ExtensionHooks
{
    public class TechnoExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x6F3260, Size = 5)]
        static public unsafe UInt32 TechnoClass_CTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6F4500, Size = 5)]
        static public unsafe UInt32 TechnoClass_DTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C250, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x70BF50, Size = 5)]
        static public unsafe UInt32 TechnoClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C249, Size = 5)]
        static public unsafe UInt32 TechnoClass_Load_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C264, Size = 5)]
        static public unsafe UInt32 TechnoClass_Save_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Save_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x6F9039, Size = 5)]
        public static unsafe UInt32 TechnoClass_Greatest_Threat_HealWeaponRange(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            int guardRange = pTechno.Ref.Type.Ref.GuardRange;
            Pointer<WeaponStruct> pirmary = pTechno.Ref.GetWeapon(0);
            if (!pirmary.IsNull && !pirmary.Ref.WeaponType.IsNull)
            {
                int range = pirmary.Ref.WeaponType.Ref.Range;
                if (range > guardRange)
                {
                    guardRange = range;
                }
            }
            Pointer<WeaponStruct> secondary = pTechno.Ref.GetWeapon(1);
            if (!secondary.IsNull && !secondary.Ref.WeaponType.IsNull)
            {
                int range = secondary.Ref.WeaponType.Ref.Range;
                if (range > guardRange)
                {
                    guardRange = range;
                }
            }
            R->EDI = (uint)guardRange;
            return 0x6F903E;
        }

        [Hook(HookType.AresHook, Address = 0x7067F1, Size = 6)]
        public static unsafe UInt32 TechnoClass_DrawVxl_DisableCache(REGISTERS* R)
        {
            if (R->ESI != R->EAX)
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                // Logger.Log($"{Game.CurrentFrame} 渲染VXL，[{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno}，检查是否使用缓存 {(int)R->ESI} {(int)R->EAX} {R->ECX} {R->Stack<int>(0x5C)}");
                if (pTechno.TryGetStatus(out TechnoStatusScript technoStatus) && technoStatus.DisableVoxelCache)
                {
                    // Logger.Log($"{Game.CurrentFrame} 渲染VXL，[{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno}，强制禁用缓存");
                    // 强制禁用缓存
                    return 0x706875;
                }
                return 0x7067F7;
            }
            return 0x706879;
        }

        #region Techno shadow resize in air
        [Hook(HookType.AresHook, Address = 0x73C4FF, Size = 5)]
        public static unsafe UInt32 UnitClass_DrawShadow_InAir(REGISTERS* R)
        {
            // Logger.Log($"{Game.CurrentFrame} 渲染载具影子 {R->EBP}, 矩阵 = {R->EAX}");
            Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
            Pointer<Matrix3DStruct> pMatrix3D = (IntPtr)R->EAX;
            // Logger.Log($"{Game.CurrentFrame} 渲染载具[{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno}的矩阵调整前 RX = {pMatrix3D.Ref.GetXRotation()} RY = {pMatrix3D.Ref.GetYRotation()} RZ = {pMatrix3D.Ref.GetZRotation()}");
            if (pTechno.TryGetStatus(out TechnoStatusScript status))
            {
                pMatrix3D.Ref.Scale(status.VoxelShadowScaleInAir);
            }
            // 调整倾斜时影子的纵向比例
            if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
            {
                Matrix3DStruct bodyMatrix3D = new Matrix3DStruct();
                pFoot.Ref.Locomotor.Draw_Matrix(Pointer<Matrix3DStruct>.AsPointer(ref bodyMatrix3D), IntPtr.Zero);
                double scale = Math.Cos(Math.Abs(bodyMatrix3D.GetYRotation()));
                pMatrix3D.Ref.ScaleX((float)scale);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x414876, Size = 7)]
        public static unsafe UInt32 AircraftClass_DrawShadow(REGISTERS* R)
        {
            // Logger.Log($"{Game.CurrentFrame} 渲染飞机影子 {R->EBP}, 矩阵 = {R->EAX}");
            Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
            Pointer<Matrix3DStruct> pMatrix3D = (IntPtr)R->EAX;
            if (pTechno.TryGetStatus(out TechnoStatusScript status))
            {
                pMatrix3D.Ref.Scale(status.VoxelShadowScaleInAir);
            }
            // 调整倾斜时影子的纵向比例
            if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
            {
                Matrix3DStruct bodyMatrix3D = new Matrix3DStruct();
                pFoot.Ref.Locomotor.Draw_Matrix(Pointer<Matrix3DStruct>.AsPointer(ref bodyMatrix3D), IntPtr.Zero);
                double scale = Math.Cos(Math.Abs(bodyMatrix3D.GetYRotation()));
                pMatrix3D.Ref.ScaleX((float)scale);
            }
            // 调整姿态变化时影子的纵向比例
            if (pTechno.TryGetComponent<AircraftAttitudeScript>(out AircraftAttitudeScript attitudeScript) && attitudeScript.PitchAngle != 0)
            {
                double scale = Math.Cos(Math.Abs(attitudeScript.PitchAngle));
                pMatrix3D.Ref.ScaleX((float)scale);
                // Logger.Log($"{Game.CurrentFrame} 飞机 {pTechno} 抬头角度 {attitudeScript.PitchAngle} 影子缩放倍率 {scale}");
            }
            return 0;
        }
        #endregion

        #region Draw colour
        // case VISUAL_NORMAL
        [Hook(HookType.AresHook, Address = 0x7063FF, Size = 7)]
        public static unsafe UInt32 TechnoClass_DrawSHP_Colour(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                // ext?.DrawSHP_Colour(R);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x706640, Size = 5)]
        public static unsafe UInt32 TechnoClass_DrawVXL_Colour(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                // Pointer<Point2D> pPos = R->Stack<IntPtr>(0x18);
                // uint bright = R->Stack<uint>(0x20);
                // uint tint = R->Stack<uint>(0x24);
                // R->Stack<uint>(0x20, 500);
                // R->Stack<uint>(0x24, ExHelper.ColorAdd2RGB565(new ColorStruct(255, 0, 0)));
                // Logger.Log($"{Game.CurrentFrame} - Techno {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] vxl draw. Pos = {R->Stack<uint>(0x18)}, Bright = {bright}, Tint = {tint}");
                // Only for Building's turret
                if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                {
                    TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                    // ext?.DrawVXL_Colour(R, true);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // after Techno_DrawVXL change berzerk color
        [Hook(HookType.AresHook, Address = 0x73C15F, Size = 7)]
        public static unsafe UInt32 UnitClass_DrawVXL_Colour(REGISTERS* R)
        {
            try
            {
                Pointer<UnitClass> pUnit = (IntPtr)R->EBP;
                // uint bright = R->Stack<uint>(0x1E0);
                // uint tint = R->ESI;
                // Logger.Log($"{Game.CurrentFrame} - Unit {pUnit.Ref.Type.Ref.Base.Base.Base.ID} vxl draw. Bright = {bright}, Tint = {tint}");
                TechnoExt ext = TechnoExt.ExtMap.Find(pUnit.Convert<TechnoClass>());
                // ext?.DrawVXL_Colour(R, false);
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        #endregion

        #region 傻逼飞机
        [Hook(HookType.AresHook, Address = 0x414971, Size = 5)]
        public static unsafe UInt32 AircraftClass_DrawIt_PitchAngle(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
            if (pTechno.TryGetComponent<AircraftAttitudeScript>(out AircraftAttitudeScript attitude) && attitude.PitchAngle != 0)
            {
                Pointer<Matrix3DStruct> pMatrix3D = (IntPtr)R->EAX;
                pMatrix3D.Ref.RotateY(attitude.PitchAngle);
                // Logger.Log($"{Game.CurrentFrame} 飞机 {R->EBP} 渲染, 调整俯仰角度 {attitude.PitchAngle}, 原始位置 {pTechno.Ref.Base.Base.GetCoords()}");
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4CF4C9, Size = 6)]
        public static unsafe UInt32 FlyLocomotionClass_FlightLevel_ResetA(REGISTERS* R)
        {
            Pointer<FlyLocomotionClass> pFly = (IntPtr)R->EBP;
            // Logger.Log($"{Game.CurrentFrame} FlyLoco {R->EBP} 重置飞行高度A {pFly.Ref.FlightLevel}");
            Pointer<TechnoClass> pTechno = pFly.Convert<LocomotionClass>().Ref.LinkedTo.Convert<TechnoClass>();
            if (pTechno.TryGetComponent<AircraftDiveScript>(out AircraftDiveScript diveScript) && diveScript.DiveStatus == AircraftDiveStatus.DIVEING)
            {
                return 0x4CF4D2;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4CF3E3, Size = 9)]
        public static unsafe UInt32 FlyLocomotionClass_FlightLevel_ResetB(REGISTERS* R)
        {
            Pointer<FlyLocomotionClass> pFly = (IntPtr)R->EBP;
            // Logger.Log($"{Game.CurrentFrame} FlyLoco {R->EBP} 重置飞行高度B {pFly.Ref.FlightLevel}");
            Pointer<TechnoClass> pTechno = pFly.Convert<LocomotionClass>().Ref.LinkedTo.Convert<TechnoClass>();
            if (pTechno.TryGetComponent<AircraftDiveScript>(out AircraftDiveScript diveScript) && diveScript.DiveStatus == AircraftDiveStatus.DIVEING)
            {
                return 0x4CF4D2;
            }
            return 0;
        }

        #endregion

    }
}