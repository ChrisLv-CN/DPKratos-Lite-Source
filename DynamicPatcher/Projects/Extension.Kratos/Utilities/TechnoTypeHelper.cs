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

    [Serializable]
    public enum LocoType
    {
        None = 0,
        Drive = 1,
        Hover = 2,
        Tunnel = 3,
        Walk = 4,
        Droppod = 5,
        Fly = 6,
        Teleport = 7,
        Mech = 8,
        Ship = 9,
        Jumpjet = 10,
        Rocket = 11
    }

    public static class TechnoTypeHelper
    {

        public static TechnoTypeData GetData(this Pointer<TechnoTypeClass> pTechnoType)
        {
            return pTechnoType.GetData(out TechnoTypeExt ext);
        }

        public static TechnoTypeData GetData(this Pointer<TechnoTypeClass> pTechnoType, out TechnoTypeExt typeExt)
        {
            typeExt = TechnoTypeExt.ExtMap.Find(pTechnoType);
            if (null != typeExt)
            {
                IConfigWrapper<TechnoTypeData> data = (IConfigWrapper<TechnoTypeData>)typeExt.TechnoTypeData;
                if (null == data)
                {
                    data = Ini.GetConfig<TechnoTypeData>(Ini.RulesDependency, pTechnoType.Ref.Base.Base.ID);
                    typeExt.TechnoTypeData = (INIComponent)data;
                }
                return data.Data;
            }
            return new TechnoTypeData();
        }

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
            WarheadTypeData data = pWH.GetData();
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

        public static bool CanBeBase(this Pointer<TechnoClass> pTechno, BaseNormalData data, int houseIndex, int minX, int maxX, int minY, int maxY)
        {
            // 检查位置在范围内
            CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
            CellStruct cellPos = MapClass.Coord2Cell(location);
            if (cellPos.X >= minX && cellPos.X <= maxX && cellPos.Y >= minY && cellPos.Y <= maxY)
            {
                // 判断所属
                Pointer<HouseClass> pTargetHouse = pTechno.Ref.Owner;
                return pTargetHouse.Ref.ArrayIndex == houseIndex || (data.EligibileForAllyBuilding && pTargetHouse.Ref.IsAlliedWith(houseIndex));
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

        public static LocoType WhatLocoType(this Pointer<TechnoClass> pTechno)
        {
            if (pTechno.CastToFoot(out Pointer<FootClass> pFoot))
            {
                return pFoot.WhatLocoType();
            }
            return LocoType.None;
        }

        public static LocoType WhatLocoType(this Pointer<FootClass> pFoot)
        {
            Pointer<LocomotionClass> pLoco = pFoot.Ref.Locomotor.ToLocomotionClass();
            Guid locoId = pLoco.Ref.GUID;
            if (locoId == LocomotionClass.Drive)
            {
                return LocoType.Drive;
            }
            else if (locoId == LocomotionClass.Hover)
            {
                return LocoType.Hover;
            }
            else if (locoId == LocomotionClass.Tunnel)
            {
                return LocoType.Tunnel;
            }
            else if (locoId == LocomotionClass.Walk)
            {
                return LocoType.Walk;
            }
            else if (locoId == LocomotionClass.Droppod)
            {
                return LocoType.Droppod;
            }
            else if (locoId == LocomotionClass.Fly)
            {
                return LocoType.Fly;
            }
            else if (locoId == LocomotionClass.Teleport)
            {
                return LocoType.Teleport;
            }
            else if (locoId == LocomotionClass.Mech)
            {
                return LocoType.Mech;
            }
            else if (locoId == LocomotionClass.Ship)
            {
                return LocoType.Ship;
            }
            else if (locoId == LocomotionClass.Jumpjet)
            {
                return LocoType.Jumpjet;
            }
            else if (locoId == LocomotionClass.Rocket)
            {
                return LocoType.Rocket;
            }
            return LocoType.None;
        }
    }
}
