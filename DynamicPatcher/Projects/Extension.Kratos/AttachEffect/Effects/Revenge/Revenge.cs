using System.Drawing;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public Revenge Revenge;

        private void InitRevenge()
        {
            this.Revenge = AEData.RevengeData.CreateEffect<Revenge>();
            RegisterEffect(Revenge);
        }
    }


    [Serializable]
    public class Revenge : Effect<RevengeData>
    {

        private int count;
        private int markFrame;

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            // 过滤平民
            Pointer<TechnoClass> pTechno = pOwner.Convert<TechnoClass>();
            Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
            if (Data.DeactiveWhenCivilian && !pHouse.IsNull && pHouse.IsCivilian())
            {
                return;
            }
            // 检查复仇者
            Pointer<TechnoClass> pRevenger = pTechno; // 复仇者
            Pointer<HouseClass> pRevengerHouse = pHouse; // 复仇者的阵营
            if (Data.FromSource)
            {
                pRevenger = AE.pSource;
                pRevengerHouse = AE.pSourceHouse;

                if (pRevenger.IsDeadOrInvisible())
                {
                    // 复仇者不存在，复个屁
                    Disable(default);
                    return;
                }
            }
            // 检查报复对象
            Pointer<TechnoClass> pRevengeTargetTechno = IntPtr.Zero; // 报复对象
            // 向AE的来源复仇
            if (Data.ToSource)
            {
                pRevengeTargetTechno = AE.pSource;
            }
            else if (!pAttacker.IsNull && pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno))
            {
                pRevengeTargetTechno = pAttackerTechno;
            }
            if (pRevengeTargetTechno.IsNull)
            {
                // 报复对象不存在
                return;
            }
            // 准备报复
            if (!pRevengeTargetTechno.IsDeadOrInvisible() && (Data.Realtime || damageState == DamageState.NowDead)
                && Data.IsOnMark(pWH)
                && !pWH.GetData().IgnoreRevenge)
            {
                // 过滤浮空
                if (!Data.AffectInAir && pRevengeTargetTechno.InAir())
                {
                    return;
                }
                // 发射武器复仇
                if (Data.CanAffectHouse(pRevengerHouse, pAttackingHouse) && Data.CanAffectType(pRevengeTargetTechno) && Data.IsOnMark(pRevengeTargetTechno))
                {
                    // 检查持续帧内触发
                    if (Data.ActiveOnce)
                    {
                        int currentFrame = Game.CurrentFrame;
                        if (markFrame == 0)
                        {
                            markFrame = currentFrame;
                        }
                        if (currentFrame != markFrame)
                        {
                            Disable(default);
                            return;
                        }
                    }

                    if (Data.Chance.Bingo() && !pRevenger.IsNull && !pRevengeTargetTechno.IsDeadOrInvisible())
                    {
                        // 使用武器复仇
                        if (null != Data.Types && Data.Types.Any())
                        {
                            AttachFireScript attachFire = pRevenger.FindOrAllocate<AttachFireScript>();
                            if (null != attachFire)
                            {
                                // 发射武器
                                foreach (string weaponId in Data.Types)
                                {
                                    if (!weaponId.IsNullOrEmptyOrNone())
                                    {
                                        attachFire.FireCustomWeapon(pRevenger, pRevengeTargetTechno.Convert<AbstractClass>(), pRevengerHouse, weaponId, Data.FireFLH, !Data.IsOnTurret, Data.IsOnTarget);
                                    }
                                }
                            }
                        }
                        // 使用自身武器复仇
                        if (Data.WeaponIndex > -1)
                        {
                            Pointer<WeaponStruct> pWeaponStruct = pTechno.Ref.GetWeapon(Data.WeaponIndex);
                            if (!pWeaponStruct.IsNull && !pWeaponStruct.Ref.WeaponType.IsNull)
                            {
                                AttachFireScript attachFire = pRevenger.FindOrAllocate<AttachFireScript>();
                                if (null != attachFire)
                                {
                                    Pointer<WeaponTypeClass> pWeapon = pWeaponStruct.Ref.WeaponType;
                                    WeaponTypeData weaponTypeData = pWeapon.GetData();
                                    // 发射武器
                                    attachFire.FireCustomWeapon(pRevenger, pRevengeTargetTechno.Convert<AbstractClass>(), pRevengerHouse, pWeapon, weaponTypeData, Data.FireFLH);
                                }
                            }
                        }
                        // 使用AE复仇
                        if (null != Data.AttachEffects && Data.AttachEffects.Any() && pRevengeTargetTechno.TryGetAEManager(out AttachEffectScript aeManager))
                        {
                            aeManager.Attach(Data.AttachEffects, Data.AttachChances, pRevenger.Convert<ObjectClass>(), pRevengerHouse);
                        }
                    }
                    // 检查触发次数
                    if (Data.TriggeredTimes > 0 && ++count >= Data.TriggeredTimes)
                    {
                        Disable(default);
                    }
                }
            }
        }

    }
}
