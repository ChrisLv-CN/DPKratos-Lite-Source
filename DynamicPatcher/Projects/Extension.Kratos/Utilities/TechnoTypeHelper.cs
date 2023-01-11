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
            return pTechno.Convert<ObjectClass>().GetRealDamage(damage, pWH, ignoreArmor, distance);
        }

        public static int GetRealDamage(this Pointer<ObjectClass> pObject, int damage, Pointer<WarheadTypeClass> pWH, bool ignoreArmor = true, int distance = 0)
        {
            int realDamage = damage;
            if (!ignoreArmor)
            {
                // 计算实际伤害
                if (realDamage > 0)
                {
                    realDamage = MapClass.GetTotalDamage(damage, pWH, pObject.Ref.Type.Ref.Armor, distance);
                }
                else
                {
                    realDamage = -MapClass.GetTotalDamage(-damage, pWH, pObject.Ref.Type.Ref.Armor, distance);
                }
            }
            return realDamage;
        }

        public static bool CanAffectMe(this Pointer<TechnoClass> pTechno, Pointer<HouseClass> pAttackingHouse, Pointer<WarheadTypeClass> pWH)
        {
            return pTechno.CanAffectMe(pAttackingHouse, pWH, out WarheadTypeData whData);
        }

        public static bool CanAffectMe(this Pointer<TechnoClass> pTechno, Pointer<HouseClass> pAttackingHouse, Pointer<WarheadTypeClass> pWH, out WarheadTypeData whData)
        {
            Pointer<HouseClass> pHosue = pTechno.Ref.Owner;
            return pWH.CanAffectHouse(pHosue, pAttackingHouse, out whData);
        }

        public static bool CanDamageMe(this Pointer<TechnoClass> pTechno, int damage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH)
        {
            return pTechno.CanDamageMe(damage, distanceFromEpicenter, pWH, out int realDamage);
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

        public static bool CanBeBase(this Pointer<TechnoClass> pTechno, int houseIndex, Pointer<CellClass> pCell, bool checkInAir = false)
        {
            Pointer<HouseClass> pTargetHouse = IntPtr.Zero;
            if (!pTechno.IsDeadOrInvisible() && (!checkInAir || pTechno.InAir()) && BaseNormalData.CanBeBase(pTechno.Ref.Type.Ref.Base.Base.ID) && !(pTargetHouse = pTechno.Ref.Owner).IsNull)
            {
                // 检查所属
                if (pTargetHouse.Ref.ArrayIndex == houseIndex || (RulesClass.Global().BuildOffAlly && pTargetHouse.Ref.IsAlliedWith(houseIndex)))
                {
                    // 检查距离
                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                    return MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pTargetCell) && pTargetCell == pCell;
                }
            }
            return false;
        }

        public static bool CanAttack(this Pointer<TechnoClass> pTechno, Pointer<AbstractClass> pTarget, bool isPassiveAcquire = false)
        {
            if (pTarget.CastToTechno(out Pointer<TechnoClass> pTargetTechno))
            {
                return pTechno.CanAttack(pTargetTechno, isPassiveAcquire);
            }
            return true;
        }

        public static bool CanAttack(this Pointer<TechnoClass> pTechno, Pointer<TechnoClass> pTarget, bool isPassiveAcquire = false)
        {
            bool canAttack = false;
            Pointer<AbstractClass> pTargetAbs = pTarget.Convert<AbstractClass>();
            int weaponIdx = pTechno.Ref.SelectWeapon(pTargetAbs);
            Pointer<WeaponStruct> pWeaponStruct = pTechno.Ref.GetWeapon(weaponIdx);
            Pointer<WeaponTypeClass> pWeapon = IntPtr.Zero;
            if (!pWeaponStruct.IsNull && !(pWeapon = pWeaponStruct.Ref.WeaponType).IsNull)
            {
                // 判断护甲
                double versus = pWeapon.Ref.Warhead.GetData().GetVersus(pTarget.Ref.Type.Ref.Base.Armor, out bool forceFire, out bool retaliate, out bool passiveAcquire);
                if (isPassiveAcquire)
                {
                    // 是否可以主动攻击
                    canAttack = versus > 0.2 || passiveAcquire;
                }
                else
                {
                    canAttack = versus != 0.0;
                }
                // 检查是否可以攻击
                if (canAttack)
                {
                    FireError fireError = pTechno.Ref.GetFireError(pTargetAbs, weaponIdx, true);
                    switch (fireError)
                    {
                        case FireError.ILLEGAL:
                        case FireError.CANT:
                            canAttack = false;
                            break;
                    }
                }
            }
            else
            {
                // 没有可以用的武器
                canAttack = false;
            }
            return canAttack;
        }
    }
}
