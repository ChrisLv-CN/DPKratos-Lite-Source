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


    }
}
