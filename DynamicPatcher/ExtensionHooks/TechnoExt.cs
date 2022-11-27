using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.FileFormats;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;
using Extension.Script;

namespace ExtensionHooks
{
    public class TechnoExtHooks
    {
        [Hook(HookType.AresHook, Address = 0x6F3260, Size = 5)]
        public static unsafe UInt32 TechnoClass_CTOR(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_CTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x6F4500, Size = 5)]
        public static unsafe UInt32 TechnoClass_DTOR(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;

                TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
                ext.GameObject.Foreach(c => (c as IObjectScriptable)?.OnUnInit());
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return TechnoExt.TechnoClass_DTOR(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C250, Size = 8)]
        [Hook(HookType.AresHook, Address = 0x70BF50, Size = 5)]
        public static unsafe UInt32 TechnoClass_SaveLoad_Prefix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_SaveLoad_Prefix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C249, Size = 5)]
        public static unsafe UInt32 TechnoClass_Load_Suffix(REGISTERS* R)
        {
            return TechnoExt.TechnoClass_Load_Suffix(R);
        }

        [Hook(HookType.AresHook, Address = 0x70C264, Size = 5)]
        public static unsafe UInt32 TechnoClass_Save_Suffix(REGISTERS* R)
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

        // if (pDamage >= 0 && pDamage < 1) pDamage = 1; // ╮(-△-)╭
        [Hook(HookType.AresHook, Address = 0x7019DD, Size = 6)]
        public static unsafe UInt32 TechnoClass_ReceiveDamage_AtLeast1(REGISTERS* R)
        {
            // var pDamage = (Pointer<int>)R->EBX;
            // Logger.Log($"{Game.CurrentFrame} - 免疫伤害， {pDamage.Ref}");
            // Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            // TechnoExt ext = TechnoExt.ExtMap.Find(pTechno);
            // if (ext.DamageReactionState.IsActive())
            // {
            return 0x7019E3;
            // }
            // return 0;
        }

        [Obsolete]
        // Will crash the game. change to use temporalUpdate check range and let go.
        // [Hook(HookType.AresHook, Address = 0x6FC163, Size = 5)]
        public static unsafe UInt32 TechnoClass_CanFire_Temporal_CheckRange(REGISTERS* R)
        {
            bool checkRange = R->Stack<bool>(0x2C);
            if (checkRange)
            {
                // cmp ebx, [eax+28h]      pTarget == Temporal.Target
                Pointer<AbstractClass> pTarget = (IntPtr)R->EBX;
                Pointer<TemporalClass> temporal = (IntPtr)R->EAX;
                if (pTarget == temporal.Ref.Target.Convert<AbstractClass>())
                {
                    // check range
                    Pointer<TechnoClass> pTechno = temporal.Ref.Owner;
                    if (!pTechno.IsDeadOrInvisible())
                    {
                        int weaponIdx = R->Stack<int>(0x28);
                        // Logger.Log($"{Game.CurrentFrame} 超时空武器检查目标是否相同 Techno = [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} == {temporal.Ref.Owner}, wapIdx = {weaponIdx}, {pTechno.Ref.IsCloseEnough(pTarget, weaponIdx)}");
                        if (!pTechno.Ref.IsCloseEnough(pTarget, weaponIdx))
                        {
                            return 0x6FCD0E; // RANGE
                        }
                    }
                    return 0x6FC168; // REARM
                }
                else
                {
                    return 0x6FC177; // goto next check
                }
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6FDD61, Size = 5)]
        public static unsafe UInt32 TechnoClass_Fire_OverrideWeapon(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status)
                    && status.OverrideWeaponState.TryGetOverrideWeapon(pTechno.Ref.Veterancy.IsElite(), out Pointer<WeaponTypeClass> pOverrideWeapon)
                    && !pOverrideWeapon.IsNull)
                {
                    // Logger.Log("Override weapon {0}", pWeapon.Ref.Base.ID);
                    R->EBX = (uint)pOverrideWeapon;
                    return 0x6FDD71;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x6FF28F, Size = 6)]
        public static unsafe UInt32 TechnoClass_Fire_ROFMultiplier(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetComponent<AutoFireAreaWeaponScript>(out AutoFireAreaWeaponScript autoFire) && autoFire.SkipROF)
                {
                    return 0x6FF2BE; // skip ROF
                }
                Pointer<WeaponTypeClass> pWeapon = (IntPtr)R->EBX;
                if (pTechno.Ref.CurrentBurstIndex >= pWeapon.Ref.Burst && pTechno.TryGetAEManager(out AttachEffectScript aeManager))
                {
                    int rof = (int)R->EAX;
                    double rofMult = aeManager.CountAttachStatusMultiplier().ROFMultiplier;
                    // Logger.Log($"{Game.CurrentFrame} Techno [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} fire done, weapon [{pWeapon.Ref.Base.ID}], burst {pTechno.Ref.CurrentBurstIndex}, ROF {rof}, ROFMult {rofMult}");
                    R->EAX = (uint)(rof * rofMult);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #region UnitClass SHP IFV
        [Hook(HookType.AresHook, Address = 0x73CD01, Size = 5)]
        public static unsafe UInt32 UnitClass_Drawcode_ChangeTurret(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
            if (!pTechno.Ref.IsVoxel() && pTechno.Ref.Type.Ref.Gunner && pTechno.Ref.Passengers.NumPassengers > 0)
            {
                Pointer<ObjectClass> pPassenger = pTechno.Ref.Passengers.FirstPassenger.Convert<ObjectClass>();
                Pointer<ObjectClass> pGunner = pPassenger;
                do
                {
                    if (!pPassenger.IsNull)
                    {
                        pGunner = pPassenger;
                    }
                } while (!pPassenger.IsNull && !(pPassenger = pPassenger.Ref.NextObject).IsNull);
                int ifvMode = pGunner.Convert<TechnoClass>().Ref.Type.Ref.IFVMode;
                // Logger.Log($"第一个乘客是 {pGunner.Ref.Type.Ref.Base.ID}, ifvMode = {ifvMode}");
                SHPFVTurretTypeData typeData = Ini.GetConfig<SHPFVTurretTypeData>(Ini.RulesDependency, pTechno.Ref.Type.Ref.Base.Base.ID).Data;
                if (typeData.TryGetData(ifvMode + 1, out SHPFVTurretData data))
                {
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{pTechno.Ref.Base.Type.Ref.Base.ID}]{pTechno} 画自定义炮塔，帧号{data.WeaponTurretFrameIndex}");
                    R->EAX += (uint)data.WeaponTurretFrameIndex;
                    if (!data.WeaponTurretCustomSHP.IsNullOrEmptyOrNone())
                    {
                        if (FileSystem.TyrLoadSHPFile(data.WeaponTurretCustomSHP, out Pointer<SHPStruct> pCustomSHP))
                        {
                            // Logger.Log($"{Game.CurrentFrame} 单位 [{pTechno.Ref.Base.Type.Ref.Base.ID}]{pTechno} 画自定义炮塔，文件{data.WeaponTurretCustomSHP}，帧号{R->EAX}");
                            R->EDI = (uint)pCustomSHP;
                        }
                    }
                    R->ECX = R->EBP;
                    return 0x73CD06;
                }
            }
            return 0;
        }
        #endregion

        #region UnitClass Deployed
        [Hook(HookType.AresHook, Address = 0x6FF929, Size = 6)]
        public static unsafe UInt32 TechnoClass_Fire_FireOnce(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
                if (pTechno.TryGetComponent<DeployFireOnceScript>(out DeployFireOnceScript deployer))
                {
                    deployer.UnitDeployFireOnce();
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // [Hook(HookType.AresHook, Address = 0x739B6A, Size = 6)] // Has Anim
        // [Hook(HookType.AresHook, Address = 0x739C6A, Size = 6)] // No Anim
        // Phobos Skip ↑ those address.
        [Hook(HookType.AresHook, Address = 0x739C74, Size = 6)]
        public static unsafe UInt32 UnitClass_Deployed(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetComponent<UnitDeployerScript>(out UnitDeployerScript deployer))
                {
                    deployer.UnitDeployToTransform();
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        #endregion

        [Hook(HookType.AresHook, Address = 0x6FC018, Size = 6)]
        public static unsafe UInt32 TechnoClass_Select_SkipVoice(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetStatus(out TechnoStatusScript technoStatus) && technoStatus.DisableSelectVoice)
                {
                    return 0x6FC01E;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #region Techno shadow resize in air
        [Hook(HookType.AresHook, Address = 0x73C4FF, Size = 5)] // InAir
        [Hook(HookType.AresHook, Address = 0x73C595, Size = 5)] // OnGround
        public static unsafe UInt32 UnitClass_DrawShadow(REGISTERS* R)
        {
            // Logger.Log($"{Game.CurrentFrame} 渲染载具影子 {R->EBP}, 矩阵 = {R->EAX}");
            Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
            Pointer<Matrix3DStruct> pMatrix3D = (IntPtr)R->EAX;
            // Logger.Log($"{Game.CurrentFrame} 渲染载具[{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno}的矩阵调整前 RX = {pMatrix3D.Ref.GetXRotation()} RY = {pMatrix3D.Ref.GetYRotation()} RZ = {pMatrix3D.Ref.GetZRotation()}");
            if (pTechno.Ref.Type.Ref.ConsideredAircraft && pTechno.TryGetStatus(out TechnoStatusScript status))
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
            return 0;
        }
        #endregion

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

        #region Draw colour
        // case VISUAL_NORMAL
        [Hook(HookType.AresHook, Address = 0x7063FF, Size = 7)]
        public static unsafe UInt32 TechnoClass_DrawSHP_Colour(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    status.TechnoClass_DrawSHP_Paintball(R);
                    status.TechnoClass_DrawSHP_Colour(R);
                }
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
                if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building && pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    // ext?.DrawVXL_Colour(R, true);
                    status.TechnoClass_DrawVXL_Paintball(R, true);
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
                Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
                // uint bright = R->Stack<uint>(0x1E0);
                // uint tint = R->ESI;
                // Logger.Log($"{Game.CurrentFrame} - Unit {pUnit.Ref.Type.Ref.Base.Base.Base.ID} vxl draw. Bright = {bright}, Tint = {tint}");
                if (pTechno.TryGetStatus(out TechnoStatusScript status))
                {
                    // ext?.DrawVXL_Colour(R, false);
                    status.TechnoClass_DrawVXL_Paintball(R, false);
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        #endregion

        #region Techno destroy debris
        [Hook(HookType.AresHook, Address = 0x70256C, Size = 6)]
        public static unsafe UInt32 TechnoClass_Destroy_Debris_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EDI;
                // Logger.Log($"{Game.CurrentFrame} - 单位 {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] 所属 {pTechno.Ref.Owner} 死亡动画 ECX = {R->ECX} EDI = {R->EDI}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pTechno.Ref.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x7024B0, Size = 6)]
        public static unsafe UInt32 TechnoClass_Destroy_Debris_Remap2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EBX;
                // Logger.Log($"{Game.CurrentFrame} - 单位 {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] 所属 {pTechno.Ref.Owner} 死亡动画2 ECX = {R->ECX} EBX = {R->EBX}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pTechno.Ref.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        #endregion

        #region Infantry death anims
        [Hook(HookType.AresHook, Address = 0x518505, Size = 6)] // NotHutman and not set DeathAnims
        public static unsafe UInt32 Infantry_ReceiveDamage_NotHuman_DeathAnim_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pInf = (IntPtr)R->ESI;
                if (pInf.TryGetStatus(out TechnoStatusScript status) && status.PlayDestroyAnims())
                {
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{pInf.Ref.Type.Ref.Base.Base.ID}]{pInf} 受伤去死播放死亡动画");
                    pInf.Ref.Base.UnInit();
                    return 0x5185E5;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x5185C8, Size = 6)] // IsHutman and not set DeathAnims
        public static unsafe UInt32 Infantry_ReceiveDamage_DeathAnim_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pInf = (IntPtr)R->ESI;
                if (pInf.TryGetStatus(out TechnoStatusScript status) && status.PlayDestroyAnims())
                {
                    // Logger.Log($"{Game.CurrentFrame} 单位 [{pInf.Ref.Type.Ref.Base.Base.ID}]{pInf} 受伤去死播放死亡动画");
                    return 0x5185F1;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #endregion

        #region Unit explosion anims

        [Hook(HookType.AresHook, Address = 0x738749, Size = 6)]
        public static unsafe UInt32 UnitClass_Destroy_Explosion_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<UnitClass> pUnit = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EAX;
                // Logger.Log($"{Game.CurrentFrame} - 载具 {pUnit} [{pUnit.Ref.Type.Ref.Base.Base.Base.ID}] owner = {pUnit.Ref.Base.Base.Owner} 死亡动画 ECX = {R->ECX} EAX = {R->EAX}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pUnit.Ref.Base.Base.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        // Take over to Create DestroyAnim Anim
        [Hook(HookType.AresHook, Address = 0x738801, Size = 6)]
        public static unsafe UInt32 UnitClass_Destroy_DestroyAnim_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pUnit = (IntPtr)R->ESI;
                if (pUnit.TryGetStatus(out TechnoStatusScript status) && status.PlayDestroyAnims())
                {
                    return 0x73887E;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #endregion

        #region Building explosion anims

        [Hook(HookType.AresHook, Address = 0x441A26, Size = 6)]
        public static unsafe UInt32 BuildingClass_Destroy_Explosion_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<BuildingClass> pBuilding = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EBP;
                // Logger.Log($"{Game.CurrentFrame} - 建筑 {pBuilding} [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] owner = {pBuilding.Ref.Base.Owner} 死亡动画 ECX = {R->ECX} EBP = {R->EBP}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pBuilding.Ref.Base.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x441B22, Size = 6)]
        public static unsafe UInt32 BuildingClass_Destroy_Exploding_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<BuildingClass> pBuilding = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EBP;
                // Logger.Log($"{Game.CurrentFrame} - 建筑 {pBuilding} [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] owner = {pBuilding.Ref.Base.Owner} 死亡动画2 ECX = {R->ECX} EBP = {R->EBP}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pBuilding.Ref.Base.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x441D25, Size = 0xA)]
        public static unsafe UInt32 BuildingClass_Destroy_DestroyAnim_Remap(REGISTERS* R)
        {
            try
            {
                Pointer<BuildingClass> pBuilding = (IntPtr)R->ESI;
                Pointer<AnimClass> pAnim = (IntPtr)R->EBP;
                // Logger.Log($"{Game.CurrentFrame} - 建筑 {pBuilding} [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] owner = {pBuilding.Ref.Base.Owner} 摧毁动画 ECX = {R->ECX} EBP = {R->EBP}");
                if (!pAnim.IsNull)
                {
                    pAnim.Ref.Owner = pBuilding.Ref.Base.Owner;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #endregion

        #region 傻逼飞机
        // [Hook(HookType.AresHook, Address = 0x414971, Size = 5)]
        // public static unsafe UInt32 AircraftClass_DrawIt_PitchAngle(REGISTERS* R)
        // {
        //     Logger.Log($"{Game.CurrentFrame} 渲染飞机 获取到的矩阵 {R->EAX}");
        //     Pointer<TechnoClass> pTechno = (IntPtr)R->EBP;
        //     if (pTechno.TryGetComponent<AircraftAttitudeScript>(out AircraftAttitudeScript attitude) && attitude.PitchAngle != 0)
        //     {
        //         Pointer<Matrix3DStruct> pMatrix3D = (IntPtr)R->EAX;
        //         pMatrix3D.Ref.RotateY(attitude.PitchAngle);
        //         // Logger.Log($"{Game.CurrentFrame} 飞机 {R->EBP} 渲染, 调整俯仰角度 {attitude.PitchAngle}, 原始位置 {pTechno.Ref.Base.Base.GetCoords()}");
        //     }
        //     return 0;
        // }


        [Hook(HookType.AresHook, Address = 0x41A697, Size = 6)]
        public static unsafe UInt32 AircraftClass_Mission_Guard_NoTarget_Enter(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            // Logger.Log($"{Game.CurrentFrame} 傻逼飞机 [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 警戒状态，没有目标，返航");
            if (pTechno.TryGetComponent<AircraftAreaGuardScript>(out AircraftAreaGuardScript fighter)
                && fighter.IsAreaGuardRolling())
            {
                // 不返回机场，而是继续前进直到目的地
                return 0x41A6AC;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x41A96C, Size = 6)]
        public static unsafe UInt32 AircraftClass_Mission_GuardArea_NoTarget_Enter(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            // Logger.Log($"{Game.CurrentFrame} 傻逼飞机 [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 区域警戒，没有目标，返航");
            if (pTechno.TryGetComponent<AircraftAreaGuardScript>(out AircraftAreaGuardScript fighter))
            {
                // Logger.Log($"{Game.CurrentFrame} 傻逼飞机正在执行Area_Guard任务但是没有目标，试图返航");
                // 不返回机场，而是继续前进直到目的地
                fighter.StartAreaGuard();
                return 0x41A97A;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4CF780, Size = 5)]
        public static unsafe UInt32 FlyLocomotionClass_Draw_Matrix_Rolling(REGISTERS* R)
        {
            // 传入的矩阵是EAX，下一步是将临时的矩阵复制给EAX
            // Logger.Log($"{Game.CurrentFrame} FlyLoco {R->ESI - 4} 获取飞机的矩阵 {R->EAX} {R->Stack<IntPtr>(0x40)} {R->Stack<IntPtr>(0xC)} {R->Stack<Matrix3DStruct>(0x8)}");
            Pointer<FlyLocomotionClass> pFly = (IntPtr)R->ESI - 4;
            Pointer<TechnoClass> pTechno = pFly.Convert<LocomotionClass>().Ref.LinkedTo.Convert<TechnoClass>();
            if (pTechno.Ref.Type.Ref.RollAngle != 0 // 可以倾转
                && pTechno.TryGetComponent<AircraftAreaGuardScript>(out AircraftAreaGuardScript fighter)
                && fighter.State == AircraftGuardState.ROLLING)
            {
                // Logger.Log($"{Game.CurrentFrame} 傻逼飞机 [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 区域警戒状态，保持倾斜");
                // 不允许倾
                // return 0x4CF809;
                // 保持倾斜
                if (fighter.Clockwise)
                {
                    return 0x4CF7B0; // 右倾
                }
                else
                {
                    return 0x4CF7DF; // 左倾
                }
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4CF80D, Size = 5)]
        public static unsafe UInt32 FlyLocomotionClass_Draw_Matrix(REGISTERS* R)
        {
            // 传入的矩阵是EAX，下一步是将临时的矩阵复制给EAX
            // Logger.Log($"{Game.CurrentFrame} FlyLoco {R->ESI - 4} 获取飞机的矩阵 {R->EAX} {R->Stack<IntPtr>(0x40)} {R->Stack<IntPtr>(0xC)} {R->Stack<Matrix3DStruct>(0x8)}");
            Pointer<FlyLocomotionClass> pFly = (IntPtr)R->ESI - 4;
            Pointer<TechnoClass> pTechno = pFly.Convert<LocomotionClass>().Ref.LinkedTo.Convert<TechnoClass>();
            if (pTechno.TryGetComponent<AircraftAttitudeScript>(out AircraftAttitudeScript attitude) && attitude.PitchAngle != 0)
            {
                Matrix3DStruct matrix3D = R->Stack<Matrix3DStruct>(0x8);
                matrix3D.RotateY(attitude.PitchAngle);
                R->Stack(0x8, matrix3D);
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

        [Hook(HookType.AresHook, Address = 0x4CF3CB, Size = 5)]
        public static unsafe UInt32 FlyLocomotionClass_4CEFB0(REGISTERS* R)
        {
            // 调整飞机的朝向，有目标时获取目标的朝向，没有目标时获得默认朝向，此时EAX为0
            // EAX是目标DirStruct的指针
            // ECX是当前Facing的指针
            // ESI是飞机的指针的指针
            Pointer<DirStruct> pDir = (IntPtr)R->EAX;
            if (pDir.IsNull)
            {
                pDir = (IntPtr)R->EDX;
                // Logger.Log($"{Game.CurrentFrame} 更改朝向 但是没有 指向默认的右下角 {pDir.Ref.value()}");
                Pointer<TechnoClass> pTechno = ((Pointer<IntPtr>)R->ESI).Ref;
                // 如果是Spawnd就全程强制执行
                // Mission是Enter的普通飞机就不管
                if (pTechno.Ref.Base.Base.IsInAir()
                    && (pTechno.Ref.Type.Ref.Spawned || Mission.Enter != pTechno.Convert<MissionClass>().Ref.CurrentMission))
                {
                    pDir.Ref.SetValue(pTechno.Ref.TurretFacing.current().value());
                }
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x41B76E, Size = 5)]
        public static unsafe UInt32 IFlyControl_Landing_Direction(REGISTERS* R)
        {
            // Logger.Log($"{Game.CurrentFrame} 飞机降落获取朝向，要用IFlyControl {R->EDI} 和RadioClass {R->ESI}");
            Pointer<TechnoClass> pAircraft = (IntPtr)R->ESI;
            if (pAircraft.TryGetComponent<AircraftAttitudeScript>(out AircraftAttitudeScript attitude))
            {
                // 取设置的dir
                R->EAX = (uint)attitude.GetPoseDir();
                // WWSB，只支持8向
                return 0x41B7BC;
            }
            R->EAX = (uint)RulesClass.Global().PoseDir;
            return 0x41B7BC; // 这个地址会跳到下面去pop EDI
        }

        [Hook(HookType.AresHook, Address = 0x41B7BE, Size = 6)]
        public static unsafe UInt32 IFlyControl_Landing_Direction2(REGISTERS* R)
        {
            // 完成pop EDI后直接返回
            return 0x41B7B4;
        }
        #endregion

        #region ========== Stand ==========
        // AI的替身在攻击友军时会强制取消目标
        [Hook(HookType.AresHook, Address = 0x6FA39D, Size = 7)]
        public static unsafe UInt32 TechnoClass_Update_NotHuman_ClearTarget_Stand(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            // Logger.Log($"{Game.CurrentFrame} [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 取消目标");
            if (CombatDamage.Data.AllowAIAttackFriendlies || pTechno.AmIStand())
            {
                return 0x6FA472;
            }
            return 0;
        }

        // // AI是否可以攻击AI
        // [Hook(HookType.AresHook, Address = 0x6FC230, Size = 6)]
        // public static unsafe UInt32 TechnoClass_CanFire_NotHuman_Stand(REGISTERS* R)
        // {
        //     Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
        //     // Logger.Log($"{Game.CurrentFrame} [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 取消目标");
        //     if (CombatDamage.Data.AllowAIAttackFriendlies || pTechno.AmIStand())
        //     {
        //         return 0x6FC24D;
        //     }
        //     return 0;
        // }

        // 替身需要显示在上层时，修改了渲染的层，导致单位在试图攻击替身时，需要武器具备AA
        [Hook(HookType.AresHook, Address = 0x6FC749, Size = 5)]
        public static unsafe UInt32 TechnoClass_CanFire_WhichLayer_Stand(REGISTERS* R)
        {
            Layer layer = (Layer)R->EAX;
            uint inAir = 0x6FC74E;
            uint onGround = 0x6FC762;
            if (layer != Layer.Ground)
            {
                try
                {
                    Pointer<AbstractClass> pTarget = R->Stack<Pointer<AbstractClass>>(0x20 - (-0x4));
                    if (pTarget.CastToTechno(out Pointer<TechnoClass> pTechno))
                    {
                        if (pTechno.AmIStand(out StandData data))
                        {
                            if (pTechno.InAir())
                            {
                                // in air
                                return inAir;
                            }
                            // on ground
                            return onGround;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.PrintException(e);
                }
                return inAir;
            }
            return onGround;
        }

        [Hook(HookType.AresHook, Address = 0x6FF66C, Size = 6)]
        public static unsafe UInt32 TechnoClass_Fire_DecreaseAmmo(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.TryGetStatus(out TechnoStatusScript technoStatus)
                    && !technoStatus.MyMaster.IsNull
                    && null != technoStatus.StandData
                    && technoStatus.StandData.UseMasterAmmo)
                {
                    technoStatus.MyMaster.Ref.DecreaseAmmo();
                    technoStatus.MyMaster.Ref.ReloadNow();

                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }

        #region Pick Stand as Target
        [Hook(HookType.AresHook, Address = 0x6FCDBE, Size = 6)]
        public static unsafe UInt32 TechnoClass_SetTarget_Stand(REGISTERS* R)
        {
            Pointer<AbstractClass> pTarget = (IntPtr)R->EDI;
            if (!pTarget.IsNull && pTarget.CastToTechno(out Pointer<TechnoClass> pTechno)
                && pTechno.AmIStand(out TechnoStatusScript status, out StandData data)
                && (data.Immune || pTechno.Ref.Type.IsNull || pTechno.Ref.Type.Ref.Base.Insignificant || pTechno.IsImmune())
                && !status.MyMaster.IsNull
            )
            {
                // 替身处于无敌状态，目标设置为替身使者
                // Logger.Log($"{Game.CurrentFrame} 选了个无敌的替身做目标，强制换成JOJO {status.MyMaster}");
                R->EDI = (uint)status.MyMaster;
            }
            return 0;
        }

        //6F9256 6 AircraftType IsOnMap ? 6F925C : 6F9377
        [Hook(HookType.AresHook, Address = 0x4D9947, Size = 6)]
        public static unsafe UInt32 FootClass_Greatest_Threat_GetTarget(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            Pointer<AbstractClass> pTarget = (IntPtr)R->EAX;
            if (pTarget.IsNull && CombatDamage.Data.AllowAutoPickStandAsTarget)
            {
                // no target, extend find Stand
                // 搜索符合条件的替身
                // Logger.Log($"{Game.CurrentFrame}, {pTechno}自动搜索目标, 替身列表里有 {TechnoStatusScript.StandArray.Count()} 个记录");
                foreach (KeyValuePair<TechnoExt, StandData> stand in TechnoStatusScript.StandArray)
                {
                    if (!stand.Key.OwnerObject.IsNull && null != stand.Value)
                    {
                        Pointer<TechnoClass> pStand = stand.Key.OwnerObject;
                        if (!stand.Value.Immune && !pStand.Ref.Type.IsNull && !pStand.Ref.Type.Ref.Base.Insignificant && !pStand.IsImmune())
                        {
                            // 是否可以攻击，并获得攻击使用的武器序号
                            bool canFire = true;
                            pTarget = pStand.Convert<AbstractClass>();
                            int weaponIdx = pTechno.Ref.SelectWeapon(pTarget);
                            FireError fireError = pTechno.Ref.GetFireError(pTarget, weaponIdx, true);
                            switch (fireError)
                            {
                                case FireError.ILLEGAL:
                                case FireError.CANT:
                                    canFire = false;
                                    break;
                            }
                            // Logger.Log($"{Game.CurrentFrame}  {pTechno}找到的替身 [{pStand.Ref.Type.Ref.Base.Base.ID}]{pStand} 选用武器{weaponIdx} FireError = {fireError}");
                            if (canFire)
                            {
                                // 检查射程
                                canFire = pTechno.Ref.IsCloseEnough(pTarget, weaponIdx);
                                if (canFire)
                                {
                                    // 检查所属
                                    int damage = pTechno.Ref.Combat_Damage(weaponIdx);
                                    if (damage < 0)
                                    {
                                        // 维修武器
                                        canFire = pTechno.Ref.Owner.Ref.IsAlliedWith(pStand.Ref.Owner) && pStand.Ref.Base.Health < pStand.Ref.Type.Ref.Base.Strength;
                                    }
                                    else
                                    {
                                        // 只允许攻击敌人
                                        canFire = !pTechno.Ref.Owner.Ref.IsAlliedWith(pStand.Ref.Owner);
                                        if (canFire)
                                        {
                                            // 检查平民
                                            if (pStand.Ref.Owner.IsCivilian())
                                            {
                                                // Ares 的平民敌对目标
                                                canFire = Ini.GetSection(Ini.RulesDependency, pStand.Ref.Type.Ref.Base.Base.ID).Get("CivilianEnemy", false);
                                                // Ares 的反击平民
                                                if (!canFire && pTechno.Ref.Owner.AutoRepel() && !pStand.Ref.Target.IsNull && pStand.Ref.Target.CastToTechno(out Pointer<TechnoClass> pTargetTarget))
                                                {
                                                    canFire = pTechno.Ref.Owner.Ref.IsAlliedWith(pTargetTarget.Ref.Owner);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (canFire)
                                {
                                    // Pick this Stand as Target
                                    R->EAX = (uint)pStand;
                                    break;
                                }
                            }
                        }
                    }
                }

            }
            return 0;
        }

        #endregion

        #region Amin ChronoSparkle
        [Hook(HookType.AresHook, Address = 0x414C27, Size = 5)]
        public static unsafe UInt32 AircraftClass_Update_SkipCreateChronoSparkleAnimOnStand(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.AmIStand(out StandData data)
                    && null != data && data.Immune)
                {
                    return 0x414C78;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x440499, Size = 5)]
        public static unsafe UInt32 BuildingClass_Update_SkipCreateChronoSparkleAnimOnStand1(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.AmIStand(out StandData data)
                    && null != data && data.Immune)
                {
                    return 0x4404D9;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x44050C, Size = 5)]
        public static unsafe UInt32 BuildingClass_Update_SkipCreateChronoSparkleAnimOnStand2(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.AmIStand(out StandData data)
                    && null != data && data.Immune)
                {
                    return 0x44055D;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x51BB17, Size = 5)]
        public static unsafe UInt32 InfantryClass_Update_SkipCreateChronoSparkleAnimOnStand(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.AmIStand(out StandData data)
                    && null != data && data.Immune)
                {
                    return 0x51BB6E;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        [Hook(HookType.AresHook, Address = 0x736250, Size = 5)]
        public static unsafe UInt32 UnitClass_Update_SkipCreateChronoSparkleAnimOnStand(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
                if (pTechno.AmIStand(out StandData data)
                    && null != data && data.Immune)
                {
                    return 0x7362A7;
                }
            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0;
        }
        #endregion

        [Hook(HookType.AresHook, Address = 0x5F6B90, Size = 5)]
        public static unsafe UInt32 ObjectClass_InAir_SkipCheckIsOnMap(REGISTERS* R)
        {
            Pointer<ObjectClass> pObject = (IntPtr)R->ECX;
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno) && pTechno.AmIStand())
            {
                return 0x5F6B97;
            }
            return 0;
        }

        // Stand can't set destination
        // BlackHole's victim can't set destination
        // Jumping unit can't set destination
        [Hook(HookType.AresHook, Address = 0x4D94B0, Size = 5)]
        public static unsafe UInt32 FootClass_SetDestination_Stand(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            if (pTechno.TryGetStatus(out TechnoStatusScript status) && (status.AmIStand() || status.CaptureByBlackHole || status.Jumping || status.CantMove))
            {
                // 跳过目的地设置
                // Logger.Log($"{Game.CurrentFrame} 跳过替身 [{pTechno.Ref.Type.Ref.Base.Base.ID}]{pTechno} 的目的地设置");
                return 0x4D9711;
            }
            return 0;
        }

        // Can't do anything, like EMP impact
        [Hook(HookType.AresHook, Address = 0x70EFD0, Size = 6)]
        public static unsafe UInt32 TechnoClass_CantMove_Stand(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            if (pTechno.TryGetStatus(out TechnoStatusScript status) && status.CantMove)
            {
                // 不允许移动
                R->AL = Convert.ToByte(true);
                return 0x70EFDA;
            }
            return 0;
        }

        // Stand support radar
        [Hook(HookType.AresHook, Address = 0x508EC6, Size = 5)]
        public static unsafe UInt32 HouseClass_RadarUpdate_Stand(REGISTERS* R)
        {
            // isOnMap = 0x508ECD, noOnMap = 0x508F08
            Pointer<TechnoClass> pTechno = (IntPtr)R->EAX;
            if (pTechno.Ref.Base.IsOnMap || pTechno.AmIStand())
            {
                return 0x508ECD;
            }
            return 0x508F08;
        }

        // Stand support spysat
        [Hook(HookType.AresHook, Address = 0x508F9B, Size = 5)]
        public static unsafe UInt32 HouseClass_SpySatUpdate_Stand(REGISTERS* R)
        {
            // isOnMap = 0x508FA2, noOnMap = 0x508FF6
            Pointer<TechnoClass> pTechno = (IntPtr)R->ECX;
            if (pTechno.Ref.Base.IsOnMap || pTechno.AmIStand())
            {
                return 0x508FA2;
            }
            return 0x508FF6;
        }

        #region Stand Drawing
        // Loco.IsMoving() == false, won't draw moving Anim
        [Hook(HookType.AresHook, Address = 0x73C69D, Size = 6)]
        public static unsafe UInt32 UnitClass_DrawSHP_Moving_Stand(REGISTERS* R)
        {
            Pointer<TechnoClass> pUnit = (IntPtr)R->ESI;
            if (pUnit.TryGetStatus(out TechnoStatusScript status) && status.StandIsMoving)
            {
                // Logger.Log($"{Game.CurrentFrame} 单位 [{pUnit.Ref.Type.Ref.Base.Base.ID}]{pUnit} 替身在移动 {R->EDX}");
                return 0x73C702;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x704363, Size = 5)]
        public static unsafe UInt32 TechnoClass_GetZAdjust_Stand(REGISTERS* R)
        {

            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            int height = (int)R->EAX;
            if (pTechno.AmIStand(out StandData data) && (null == data || !data.IsTrain))
            {
                int ZAdjust = TacticalClass.Instance.Ref.AdjustForZ(height);
                int offset = null != data ? data.ZOffset : 14;
                // Logger.Log($"{Game.CurrentFrame} - {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] GetZAdjust EAX = {height}, AdjForZ = {ZAdjust}, offset = {offset}");
                R->ECX = (uint)(ZAdjust + offset);
                // Logger.Log("ZOffset = {0}, ECX = {1}, EAX = {2}", offset, R->ECX, R->EAX);
                return 0x704368;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x4DB7F7, Size = 6)]
        public static unsafe UInt32 FootClass_In_Which_Layer_Stand(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->ESI;
            if (pTechno.AmIStand(out StandData data) && null != data && !data.IsTrain && data.ZOffset != 0)
            {
                // Logger.Log($"{Game.CurrentFrame} - {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] StandType.DrawLayer = {ext.StandType.DrawLayer}, StandType.ZOffset = {ext.StandType.ZOffset}");
                Layer layer = data.DrawLayer;
                if (layer == Layer.None)
                {
                    // Logger.Log($" - {Game.CurrentFrame} - {pTechno} [{pTechno.Ref.Type.Ref.Base.Base.ID}] EAX = {(Layer)R->EAX} InAir = {pTechno.Ref.Base.Base.IsInAir()}");
                    R->EAX = pTechno.Ref.Base.Base.IsInAir() ? (uint)Layer.Top : (uint)Layer.Air;
                }
                else
                {
                    R->EAX = (uint)layer;
                }
                return 0x4DB803;
            }
            return 0;
        }

        [Hook(HookType.AresHook, Address = 0x54B8E9, Size = 6)]
        public static unsafe UInt32 JumpjetLocomotionClass_In_Which_Layer_Deviation(REGISTERS* R)
        {
            Pointer<TechnoClass> pTechno = (IntPtr)R->EAX;
            if (pTechno.Ref.Base.Base.IsInAir())
            {
                if (pTechno.TryGetComponent<AttachEffectScript>(out AttachEffectScript ae) && ae.HasStand())
                {
                    // Override JumpjetHeight / CruiseHeight check so it always results in 3 / Layer::Air.
                    R->EDX = Int32.MaxValue;
                    return 0x54B96B;
                }
            }
            return 0;
        }
        #endregion

        #endregion ---------- Stand ----------

        [Hook(HookType.AresHook, Address = 0x639DD8, Size = 5)]
        public static unsafe UInt32 PlanningManager_AllowAircraftsWaypoint(REGISTERS* R)
        {
            try
            {
                Pointer<TechnoClass> pTechno = (Pointer<TechnoClass>)R->ESI;
                switch (pTechno.Ref.BaseAbstract.WhatAmI())
                {
                    case AbstractType.Infantry:
                    case AbstractType.Unit:
                        return 0x639DDD;
                    case AbstractType.Aircraft:
                        if (!pTechno.Ref.Type.Ref.Spawned)
                        {
                            return 0x639DDD;
                        }
                        else
                        {
                            return 0x639E03;
                        }
                    default:
                        return 0x639E03;
                }

            }
            catch (Exception e)
            {
                Logger.PrintException(e);
            }
            return 0x639E03;
        }

    }
}
