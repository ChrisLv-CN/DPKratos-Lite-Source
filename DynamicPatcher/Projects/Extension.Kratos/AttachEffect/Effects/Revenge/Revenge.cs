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
        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pAttacker.IsNull && (Data.Realtime || damageState == DamageState.NowDead) && pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno) && !pAttacker.IsDeadOrInvisible())
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
                    // 使用武器复仇
                    if (null != Data.Types && Data.Types.Any())
                    {
                        AttachFireScript attachFire = pTechno.FindOrAllocate<AttachFireScript>();
                        if (null != attachFire)
                        {
                            Pointer<AbstractClass> pRevengTarget = pAttacker.Convert<AbstractClass>();
                            // 发射武器
                            foreach (string weaponId in Data.Types)
                            {
                                if (!weaponId.IsNullOrEmptyOrNone())
                                {
                                    attachFire.FireCustomWeapon(pTechno, pAttacker.Convert<AbstractClass>(), pHouse, weaponId, default);
                                }
                            }
                        }
                    }
                    // 使用AE复仇
                    if (null != Data.AttachEffects && Data.AttachEffects.Any())
                    {
                        AttachEffectScript aeManager = pAttackerTechno.GetComponent<AttachEffectScript>();
                        aeManager.Attach(Data.AttachEffects, pTechno.Convert<ObjectClass>(), pHouse);
                    }
                }
            }
        }

    }
}
