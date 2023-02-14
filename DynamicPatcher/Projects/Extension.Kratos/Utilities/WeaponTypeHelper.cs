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

    public static class WeaponTypeHelper
    {

        public static WeaponTypeData GetData(this Pointer<WeaponTypeClass> pWeapon)
        {
            return pWeapon.GetData(out WeaponTypeExt ext);
        }

        public static WeaponTypeData GetData(this Pointer<WeaponTypeClass> pWeapon, out WeaponTypeExt ext)
        {
            ext = WeaponTypeExt.ExtMap.Find(pWeapon);
            if (null != ext)
            {
                IConfigWrapper<WeaponTypeData> data = (IConfigWrapper<WeaponTypeData>)ext.WeaponTypeData;
                if (null == data)
                {
                    data = Ini.GetConfig<WeaponTypeData>(Ini.RulesDependency, pWeapon.Ref.Base.ID);
                    ext.WeaponTypeData = (INIComponent)data;
                }
                return data.Data;
            }
            return new WeaponTypeData();
        }

        public static bool CanFireToTarget(this WeaponTypeData weaponTypeData, Pointer<ObjectClass> pShooter, Pointer<TechnoClass> pAttacker, Pointer<AbstractClass> pTarget, Pointer<HouseClass> pAttackingHouse,
            Pointer<WeaponTypeClass> pWeapon)
        {
            bool canFire = true;
            // 检查发射者的血量
            if (weaponTypeData.CheckShooterHP)
            {
                double hp = 1;
                if (pAttacker.IsNull)
                {
                    hp = pShooter.Ref.GetHealthPercentage();
                }
                else
                {
                    hp = pAttacker.Ref.Base.GetHealthPercentage();
                }
                if (hp < weaponTypeData.OnlyFireWhenHP.X || hp > weaponTypeData.OnlyFireWhenHP.Y)
                {
                    canFire = false;
                }
            }
            // 检查目标类型
            if (canFire)
            {
                AbstractType targetAbsType = pTarget.Ref.WhatAmI();
                switch (targetAbsType)
                {
                    case AbstractType.Terrain:
                        // 检查伐木
                        if (!weaponTypeData.AffectTerrain)
                        {
                            canFire = false;
                        }
                        break;
                    case AbstractType.Cell:
                        // 检查A地板
                        if (weaponTypeData.CheckAG && !pWeapon.Ref.Projectile.Ref.AG)
                        {
                            canFire = false;
                        }
                        break;
                    case AbstractType.Building:
                    case AbstractType.Infantry:
                    case AbstractType.Unit:
                    case AbstractType.Aircraft:
                        // 检查类型
                        if (!weaponTypeData.CanAffectType(targetAbsType))
                        {
                            canFire = false;
                            break;
                        }
                        Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                        // 检查目标血量
                        if (weaponTypeData.CheckTargetHP)
                        {
                            double hp = pTargetTechno.Ref.Base.GetHealthPercentage();
                            if (hp < weaponTypeData.OnlyFireWhenTargetHP.X || hp > weaponTypeData.OnlyFireWhenTargetHP.Y)
                            {
                                canFire = false;
                                break;
                            }
                        }
                        // 检查标记
                        if (!weaponTypeData.IsOnMark(pTargetTechno))
                        {
                            canFire = false;
                            break;
                        }
                        // 检查名单
                        if (!weaponTypeData.CanAffectType(pTargetTechno.Ref.Type.Ref.Base.Base.ID))
                        {
                            canFire = false;
                            break;
                        }
                        // 检查护甲
                        if (weaponTypeData.CheckVersus && !pWeapon.Ref.Warhead.IsNull
                            && (pWeapon.Ref.Warhead.GetData().GetVersus(pTargetTechno.Ref.Type.Ref.Base.Armor, out bool forceFire, out bool retaliate, out bool passiveAcquire) == 0.0 || !forceFire)
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} 弹头对试图攻击的目标比例为0，终止发射");
                            // 护甲为零，终止发射
                            canFire = false;
                            break;
                        }
                        // 检查所属
                        Pointer<HouseClass> pTargetHouse = pTargetTechno.Ref.Owner;
                        if (!pAttackingHouse.CanAffectHouse(pTargetHouse, weaponTypeData.AffectsOwner, weaponTypeData.AffectsAllies, weaponTypeData.AffectsEnemies, weaponTypeData.AffectsCivilian))
                        {
                            // Logger.Log($"{Game.CurrentFrame} [{(pAttackingHouse.IsNull ? "null" : pAttackingHouse.Ref.ArrayIndex)}]{pAttackingHouse}不可对该所属[{(pTargetHouse.IsNull ? "null" : pTargetHouse.Ref.ArrayIndex)}]{pTargetHouse}攻击，终止发射");
                            // 不可对该所属攻击，终止发射
                            canFire = false;
                        }
                        break;
                }
            }
            return canFire;
        }

    }
}
