using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class TechnoTypeHelper
    {

        public static double GetROFMult(this Pointer<TechnoClass> pTechno)
        {
            bool rofAbility = false;
            if (pTechno.Ref.Veterancy.IsElite())
            {
                rofAbility = pTechno.Ref.Type.Ref.VeteranAbilities.ROF || pTechno.Ref.Type.Ref.EliteAbilities.ROF;
            }
            else if (pTechno.Ref.Veterancy.IsVeteran())
            {
                rofAbility = pTechno.Ref.Type.Ref.VeteranAbilities.ROF;
            }
            return !rofAbility ? 1.0 : RulesClass.Global().VeteranROF * ((pTechno.Ref.Owner.IsNull || pTechno.Ref.Owner.Ref.Type.IsNull) ? 1.0 : pTechno.Ref.Owner.Ref.Type.Ref.ROFMult);
        }

        public static double GetDamageMult(this Pointer<TechnoClass> pTechno)
        {
            if (pTechno.IsNull || !pTechno.Ref.Base.IsAlive)
            {
                return 1;
            }
            bool firepower = false;
            if (pTechno.Ref.Veterancy.IsElite())
            {
                firepower = pTechno.Ref.Type.Ref.VeteranAbilities.FIREPOWER || pTechno.Ref.Type.Ref.EliteAbilities.FIREPOWER;
            }
            else if (pTechno.Ref.Veterancy.IsVeteran())
            {
                firepower = pTechno.Ref.Type.Ref.VeteranAbilities.FIREPOWER;
            }
            return (!firepower ? 1.0 : RulesClass.Global().VeteranCombat) * pTechno.Ref.FirepowerMultiplier * ((pTechno.Ref.Owner.IsNull || pTechno.Ref.Owner.Ref.Type.IsNull) ? 1.0 : pTechno.Ref.Owner.Ref.Type.Ref.FirepowerMult);
        }


        public static int GetRealDamage(this Pointer<TechnoClass> pTechno, int damage, Pointer<WarheadTypeClass> pWH, bool ignoreArmor = true, int distance = 0)
        {
            int realDamage = damage;
            if (!ignoreArmor)
            {
                // 计算实际伤害
                if (realDamage > 0)
                {
                    realDamage = MapClass.GetTotalDamage(damage, pWH, pTechno.Ref.Base.Type.Ref.Armor, distance);
                }
                else
                {
                    realDamage = -MapClass.GetTotalDamage(-damage, pWH, pTechno.Ref.Base.Type.Ref.Armor, distance);
                }
            }
            return realDamage;
        }

        public static bool CanAffectMe(this Pointer<TechnoClass> pTechno, Pointer<HouseClass> pAttackHouse, Pointer<WarheadTypeClass> pWH)
        {
            Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
            return pWH.CanAffectHouse(pHouse, pAttackHouse);
        }

        public static bool CanDamageMe(this Pointer<TechnoClass> pTechno, int damage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH, out int realDamage, bool effectsRequireDamage = false)
        {
            // 计算实际伤害
            realDamage = pTechno.GetRealDamage(damage, pWH, false, distanceFromEpicenter);
            WarheadTypeData data = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            if (damage == 0)
            {
                return data.AllowZeroDamage;
            }
            else
            {
                if (data.EffectsRequireVerses)
                {
                    // 必须要可以造成伤害
                    if (MapClass.GetTotalDamage(RulesClass.Global().MaxDamage, pWH, pTechno.Ref.Base.Type.Ref.Armor, 0) == 0)
                    {
                        // 弹头无法对该类型护甲造成伤害
                        return false;
                    }
                    // 伤害非零，当EffectsRequireDamage=yes时，必须至少造成1点实际伤害
                    if (effectsRequireDamage || data.EffectsRequireDamage)
                    {
                        // Logger.Log("{0} 收到伤害 {1}, 弹头 {2}, 爆心距离{3}, 实际伤害{4}", OwnerObject.Ref.Type.Ref.Base.Base.ID, damage, warheadTypeExt.OwnerObject.Ref.Base.ID, distanceFromEpicenter, realDamage);
                        return realDamage != 0;
                    }
                }
            }
            return true;
        }

    }
}
