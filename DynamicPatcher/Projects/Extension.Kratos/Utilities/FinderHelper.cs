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
    public delegate bool Found<T>(Pointer<T> pTarget);
    public delegate bool FoundAEM<T>(Pointer<T> pTarget, AttachEffectScript aem);
    public delegate bool FoundIndex<T>(Pointer<T> pTarget, int index);

    public static class FinderHelper
    {

        public static void FindBulletTargetMe(this Pointer<TechnoClass> pTechno, Found<BulletClass> func)
        {
            BulletClass.Array.FindObject((pBullet) =>
            {
                // 抛射体拥有目标，且目标是自己
                Pointer<AbstractClass> pBulletTarget = pBullet.Ref.Target;
                if (!pBulletTarget.IsNull && pBulletTarget == pTechno.Convert<AbstractClass>())
                {
                    return func(pBullet);
                }
                return false;
            });
        }

        public static void FindOwnerTechno(Pointer<HouseClass> pHouse, Found<TechnoClass> func, bool allied = false, bool enemies = false)
        {
            TechnoClass.Array.FindObject(func, default, 0, pHouse, true, allied, enemies, false);
        }

        public static void FindIndex<T>(this DynamicVectorClass<Pointer<T>> array, FoundIndex<T> func)
        {
            for (int i = array.Count - 1; i >= 0; i--)
            {
                Pointer<T> pT = array.Get(i);
                if (!pT.IsNull && func(pT, i))
                {
                    break;
                }
            }
        }

        public static void FindObject<T>(this DynamicVectorClass<Pointer<T>> array, Found<T> func,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true) where T : struct
        {
            FindObject(array, func, default, 0, pHouse, owner, allied, enemies, civilian);
        }

        public static void FindObject<T>(this DynamicVectorClass<Pointer<T>> array, Found<T> func,
            CoordStruct location, double spread,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true) where T : struct
        {
            double dist = (spread <= 0 ? 1 : spread) * 256;
            for (int i = array.Count - 1; i >= 0; i--)
            {
                Pointer<T> pT = array.Get(i);
                // 分离类型
                Pointer<ObjectClass> pObject = pT.Convert<ObjectClass>();
                if (!pObject.IsNull)
                {
                    CoordStruct targetLocation = pObject.Ref.Base.GetCoords();
                    if (spread == 0 || location == default || targetLocation.DistanceFrom(location) <= dist)
                    {
                        if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
                        {
                            Pointer<HouseClass> pTargetHouse = pTechno.Ref.Owner;
                            if (!pHouse.IsNull && !pTargetHouse.IsNull
                                && (pTargetHouse == pHouse ? !owner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? !allied : !enemies)))
                            {
                                continue;
                            }
                        }
                        else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet) && pBullet.TryGetStatus(out BulletStatusScript bulletStatus) && !bulletStatus.LifeData.IsDetonate)
                        {
                            Pointer<HouseClass> pTargetHouse = IntPtr.Zero;
                            if (!pHouse.IsNull && !(pTargetHouse = bulletStatus.pSourceHouse).IsNull)
                            {
                                // 检查原始所属
                                if ((pTargetHouse.IsCivilian() && !civilian)
                                    || (pTargetHouse == pHouse ? !owner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? !allied : !enemies)))
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                        if (func(pT))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void FindFoot(Found<FootClass> func,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            FindFoot(func, default, 0, pHouse, owner, allied, enemies, civilian);
        }

        public static void FindFoot(Found<FootClass> func, CoordStruct location, double spread,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            InfantryClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, spread, pHouse, owner, allied, enemies, civilian);
            UnitClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, spread, pHouse, owner, allied, enemies, civilian);
            AircraftClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, spread, pHouse, owner, allied, enemies, civilian);
        }

        public static void FindTechnoInCell(this Pointer<CellClass> pCell, Found<TechnoClass> found)
        {
            // 获取地面
            Pointer<ObjectClass> pObject = pCell.Ref.GetContent();
            do
            {
                // Logger.Log("Object {0}, Type {1}", pObject, pObject.IsNull ? "is null" : pObject.Ref.Base.WhatAmI());
                if (!pObject.IsNull && pObject.CastToTechno(out Pointer<TechnoClass> pTarget))
                {
                    if (found(pTarget))
                    {
                        break;
                    }
                }
            }
            while (!pObject.IsNull && !(pObject = pObject.Ref.NextObject).IsNull);
        }

        public static List<Pointer<TechnoClass>> GetCellSpreadTechnos(CoordStruct location, double spread, bool includeInAir, bool ignoreBulidingOuter,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();

            CellStruct cur = MapClass.Coord2Cell(location);
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCurretCell))
            {
                cur = pCurretCell.Ref.MapCoords;
            }

            uint range = (uint)(spread + 0.99);
            CellSpreadEnumerator enumerator = new CellSpreadEnumerator(range);

            do
            {
                CellStruct offset = enumerator.Current;
                if (MapClass.Instance.TryGetCellAt(cur + offset, out Pointer<CellClass> pCell))
                {
                    pCell.FindTechnoInCell((pTarget) =>
                    {
                        Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                        if (!pHouse.IsNull && !pTargetHouse.IsNull
                            && (pTargetHouse == pHouse ? !owner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? !allied : !enemies)))
                        {
                            return false;
                        }
                        pTechnoSet.Add(pTarget);
                        return false;
                    });
                }
                // 获取JJ
                if (includeInAir && !pCell.IsNull && !pCell.Ref.Jumpjet.IsNull)
                {
                    Pointer<TechnoClass> pJJ = pCell.Ref.Jumpjet.Convert<TechnoClass>();
                    Pointer<HouseClass> pTargetHouse = pJJ.Ref.Owner;
                    if (!pHouse.IsNull && !pTargetHouse.IsNull
                        && (pTargetHouse == pHouse ? !owner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? !allied : !enemies)))
                    {
                        continue;
                    }
                    // Logger.Log($"{Game.CurrentFrame} 检索到当前格子的JJ [{pJJ.Ref.Type.Ref.Base.Base.ID}]，加入列表");
                    pTechnoSet.Add(pJJ);
                }
            } while (enumerator.MoveNext());

            // Logger.Log("range = {0}, pTechnoSet.Count = {1}", range, pTechnoSet.Count);

            if (includeInAir)
            {
                // 获取所有在天上的玩意儿，JJ，飞起来的坦克，包含路过的飞机
                FindFoot((pTarget) =>
                {
                    Pointer<TechnoClass> pTechno = pTarget.Convert<TechnoClass>();
                    if (pTechno.Ref.Base.GetHeight() > 0 && pTechno.Ref.Base.Base.GetCoords().DistanceFrom(location) <= spread * 256)
                    {
                        pTechnoSet.Add(pTechno.Convert<TechnoClass>());
                    }
                    return false;
                }, location, spread, pHouse, owner, allied, enemies, civilian);
            }
            // Logger.Log("includeAir = {0}, pTechnoSet.Count = {1}", includeInAir, pTechnoSet.Count);
            // 筛选并去掉不可用项目
            List<Pointer<TechnoClass>> pTechnoList = new List<Pointer<TechnoClass>>();
            foreach (Pointer<TechnoClass> pTechno in pTechnoSet)
            {
                CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                double dist = targetPos.DistanceFrom(location);

                bool checkDistance = true;
                AbstractType absType = pTechno.Ref.Base.Base.WhatAmI();
                switch (absType)
                {
                    case AbstractType.Building:
                        if (pTechno.Convert<BuildingClass>().Ref.Type.Ref.InvisibleInGame)
                        {
                            continue;
                        }
                        if (!ignoreBulidingOuter)
                        {
                            checkDistance = false;
                        }
                        break;
                    case AbstractType.Aircraft:
                        if (pTechno.InAir())
                        {
                            dist *= 0.5;
                        }
                        break;
                }

                if (!checkDistance || dist <= spread * 256)
                {
                    pTechnoList.Add(pTechno);
                }
            }
            return pTechnoList;
        }

        public static Pointer<TechnoClass> GetTechnoRandom(this Pointer<HouseClass> pHouse)
        {
            int maxValue = TechnoClass.Array.Count();
            if (maxValue > 0)
            {
                int targetIndex = MathEx.Random.Next(maxValue);
                Pointer<TechnoClass> pTarget = TechnoClass.Array[targetIndex];
                if (pTarget.IsNull || pTarget.Ref.Owner.IsNull || pTarget.Ref.Owner != pHouse)
                {
                    return pHouse.GetTechnoRandom();
                }
                return pTarget;
            }
            return IntPtr.Zero;
        }

        // 搜索单位
        public static void FindTechnoOnMark(FoundAEM<TechnoClass> func, CoordStruct location, double spreadMax, double spreadMin,
            Pointer<HouseClass> pHouse, FilterEffectData data, Pointer<ObjectClass> exclude)
        {
            List<Pointer<TechnoClass>> pTechnoList = null;
            if (spreadMax <= 0)
            {
                // 搜索全部单位
                HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();
                if (data.AffectBuilding)
                {
                    BuildingClass.Array.FindObject((pTarget) =>
                    {
                        if (!pTarget.Ref.Type.Ref.InvisibleInGame)
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                        }
                        return false;
                    }, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
                }
                if (data.AffectInfantry)
                {
                    InfantryClass.Array.FindObject((pTarget) =>
                    {
                        Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                        if (data.AffectInAir || !pTargetTechno.InAir())
                        {
                            pTechnoSet.Add(pTargetTechno);
                        }
                        return false;
                    }, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
                }
                if (data.AffectUnit)
                {
                    UnitClass.Array.FindObject((pTarget) =>
                    {
                        Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                        if (data.AffectInAir || !pTargetTechno.InAir())
                        {
                            pTechnoSet.Add(pTargetTechno);
                        }
                        return false;
                    }, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
                }
                if (data.AffectAircraft)
                {
                    AircraftClass.Array.FindObject((pTarget) =>
                    {
                        Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                        if (data.AffectInAir || !pTargetTechno.InAir())
                        {
                            pTechnoSet.Add(pTargetTechno);
                        }
                        return false;
                    }, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
                }
                pTechnoList = new List<Pointer<TechnoClass>>(pTechnoSet);
            }
            else
            {
                // 小范围搜索
                pTechnoList = FinderHelper.GetCellSpreadTechnos(location, spreadMax, data.AffectInAir, false, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
            }
            if (null != pTechnoList)
            {
                foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                {
                    // 检查死亡
                    if (pTarget.IsDeadOrInvisible() || pTarget.Convert<ObjectClass>() == exclude)
                    {
                        continue;
                    }
                    // 过滤替身和虚单位
                    if (pTarget.TryGetStatus(out var status) && (status.AmIStand() || status.VirtualUnit))
                    {
                        continue;
                    }
                    // 检查最小距离
                    if (spreadMin > 0)
                    {
                        double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                        if (pTarget.InAir() && pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                        {
                            distance *= 0.5;
                        }
                        if (distance < spreadMin * 256)
                        {
                            continue;
                        }
                    }
                    // 可影响
                    if (data.CanAffectType(pTarget) && pTarget.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager, data))
                    {
                        // 执行动作
                        if (func(pTarget, aeManager))
                        {
                            break;
                        }
                    }
                }
            }
        }

        // 搜索抛射体
        public static void FindBulletOnMark(FoundAEM<BulletClass> func, CoordStruct location, double spreadMax, double spreadMin,
            Pointer<HouseClass> pHouse, FilterEffectData data, Pointer<ObjectClass> exclude)
        {
            HashSet<Pointer<BulletClass>> pBulletSet = new HashSet<Pointer<BulletClass>>();
            BulletClass.Array.FindObject((pTarget) =>
            {
                if (!pTarget.IsDeadOrInvisible() && pTarget.Convert<ObjectClass>() != exclude)
                {
                    if (spreadMin > 0)
                    {
                        double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                        if (distance < spreadMin * 256)
                        {
                            return false;
                        }
                    }
                    // 可影响
                    if (data.CanAffectType(pTarget))
                    {
                        pBulletSet.Add(pTarget);
                    }
                }
                return false;
            }, location, spreadMax, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
            foreach (Pointer<BulletClass> pBullet in pBulletSet)
            {
                if (pBullet.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager, data))
                {
                    // 执行动作
                    if (func(pBullet, aeManager))
                    {
                        break;
                    }
                }
            }
        }

        private static bool IsOnMark(AttachEffectScript aeManager, FilterEffectData data)
        {
            return null == data.OnlyAffectMarks || !data.OnlyAffectMarks.Any()
                || (aeManager.TryGetMarks(out HashSet<string> marks)
                    && (data.OnlyAffectMarks.Intersect(marks).Count() > 0)
                );
        }

    }
}
