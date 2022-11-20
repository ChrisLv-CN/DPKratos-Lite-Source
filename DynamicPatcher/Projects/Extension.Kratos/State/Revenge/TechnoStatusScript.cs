using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript
    {
        public State<RevengeData> RevengeState = new State<RevengeData>();

        public void InitState_Revenge()
        {
            // 初始化状态机
            RevengeData data = Ini.GetConfig<RevengeData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                RevengeState.Enable(data);
            }
        }

        public unsafe void OnReceiveDamage2_Revenge(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (damageState == DamageState.NowDead && !pAttacker.IsNull && RevengeState.IsActive() && pAttacker.CastToTechno(out Pointer<TechnoClass> pAttackerTechno) && !pAttacker.IsDeadOrInvisible())
            {
                RevengeData data = RevengeState.Data;
                // 过滤平民
                Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                if (data.DeactiveWhenCivilian && !pHouse.IsNull && pHouse.IsCivilian())
                {
                    return;
                }
                // 过滤浮空
                if (!data.AffectInAir && pAttackerTechno.InAir())
                {
                    return;
                }
                // 发射武器复仇
                if (data.CanAffectHouse(pHouse, pAttackingHouse) && data.CanAffectType(pAttackerTechno) && data.IsOnMark(pAttackerTechno))
                {
                    // 使用武器复仇
                    if (null != data.Types && data.Types.Any())
                    {
                        AttachFireScript attachFire = pTechno.FindOrAllocate<AttachFireScript>();
                        if (null != attachFire)
                        {
                            Pointer<AbstractClass> pRevengTarget = pAttacker.Convert<AbstractClass>();
                            // 发射武器
                            foreach (string weaponId in data.Types)
                            {
                                if (!weaponId.IsNullOrEmptyOrNone())
                                {
                                    attachFire.FireCustomWeapon(pTechno, pAttacker.Convert<AbstractClass>(), pHouse, weaponId, default);
                                }
                            }
                        }
                    }
                    // 使用AE复仇
                    if (null != data.AttachEffects && data.AttachEffects.Any())
                    {
                        AttachEffectScript aeManager = pAttackerTechno.GetComponent<AttachEffectScript>();
                        aeManager.Attach(data.AttachEffects, pTechno.Convert<ObjectClass>(), pHouse);
                    }
                }
            }
        }

    }
}
