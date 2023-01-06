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
            if (!pAttacker.IsNull && (Data.Realtime || damageState == DamageState.NowDead)
                && pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno)
                && !pAttacker.IsDeadOrInvisible()
                && Data.IsOnMark(pWH)
                && pWH.CanRevenge())
            {
                // 过滤平民
                Pointer<TechnoClass> pTechno = pOwner.Convert<TechnoClass>();
                Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                if (Data.DeactiveWhenCivilian && !pHouse.IsNull && pHouse.IsCivilian())
                {
                    return;
                }
                // 过滤浮空
                if (!Data.AffectInAir && pAttackerTechno.InAir())
                {
                    return;
                }
                // 发射武器复仇
                if (Data.CanAffectHouse(pHouse, pAttackingHouse) && Data.CanAffectType(pAttackerTechno) && Data.IsOnMark(pAttackerTechno))
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
                    Pointer<TechnoClass> pRevenger = pTechno; // 复仇者
                    Pointer<HouseClass> pRevengerHouse = pHouse;
                    Pointer<TechnoClass> pRevengeTargetTechno = pAttackerTechno; // 报复对象
                    if (Data.ToSource)
                    {
                        pRevengeTargetTechno = AE.pSource;
                    }
                    if (Data.FromSource)
                    {
                        pRevenger = AE.pSource;
                        pRevengerHouse = AE.pSourceHouse;
                    }
                    if (!pRevenger.IsNull && !pRevengeTargetTechno.IsDeadOrInvisible())
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
                                        attachFire.FireCustomWeapon(pRevenger, pRevengeTargetTechno.Convert<AbstractClass>(), pRevengerHouse, weaponId, Data.FireFLH);
                                    }
                                }
                            }
                        }
                        // 使用AE复仇
                        if (null != Data.AttachEffects && Data.AttachEffects.Any() && pRevengeTargetTechno.TryGetAEManager(out AttachEffectScript aeManager))
                        {
                            aeManager.Attach(Data.AttachEffects, pRevenger.Convert<ObjectClass>(), pRevengerHouse);
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
