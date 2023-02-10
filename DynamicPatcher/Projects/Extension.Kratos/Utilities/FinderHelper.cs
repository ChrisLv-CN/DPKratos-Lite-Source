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
            Pointer<AbstractClass> pSelf = pTechno.Convert<AbstractClass>();
            foreach (BulletExt bulletExt in BulletStatusScript.TargetAircraftBullets)
            {
                if (null != bulletExt && !bulletExt.OwnerObject.IsDeadOrInvisible())
                {
                    Pointer<BulletClass> pBullet = bulletExt.OwnerObject;
                    Pointer<AbstractClass> pTarget = IntPtr.Zero;
                    if (!pBullet.IsDeadOrInvisible() && !(pTarget = pBullet.Ref.Target).IsNull)
                    {
                        if (pTarget == pSelf && func(pBullet))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public static void FindOwnerTechno(Pointer<HouseClass> pHouse, Found<TechnoClass> func, bool allied = false, bool enemies = false)
        {
            TechnoClass.Array.FindObject(func, pHouse, true, allied, enemies, false);
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
            FindObject(array, func, default, 0, 0, false, pHouse, owner, allied, enemies, civilian);
        }

        public static void FindObject<T>(this DynamicVectorClass<Pointer<T>> array, Found<T> func,
            CoordStruct location, double maxSpread, double minSpread = 0, bool fullAirspace = false,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true) where T : struct
        {
            // 最大范围小于0，搜索全部，等于0，搜索1格范围
            double maxRange = (maxSpread > 0 ? maxSpread : maxSpread == 0 ? 1 : 0) * 256;
            double minRange = (minSpread <= 0 ? 0 : minSpread) * 256;
            for (int i = array.Count - 1; i >= 0; i--)
            {
                Pointer<T> pT = array.Get(i);
                // 分离类型
                Pointer<ObjectClass> pObject = pT.Convert<ObjectClass>();
                if (!pObject.IsNull && Hit(pObject, location, maxRange, minRange, fullAirspace, pHouse, owner, allied, enemies, civilian))
                {
                    if (func(pT))
                    {
                        break;
                    }
                }
            }
        }

        private static bool Hit(Pointer<ObjectClass> pObject,
            CoordStruct location, double maxRange, double minRange, bool fullAirspace,
            Pointer<HouseClass> pHouse,
            bool owner, bool allied, bool enemies, bool civilian)
        {
            bool inRange = maxRange == 0 || default == location;
            CoordStruct targetLocation = inRange ? default : pObject.Ref.Base.GetCoords(); // 不检查距离就不用算
            bool isInAir = false; // 全空域就不用算
            AbstractType abstractType = default; // 全空域就不用算
            if (!fullAirspace)
            {
                isInAir = pObject.Ref.Base.IsInAir();
                abstractType = pObject.Ref.Base.WhatAmI();
            }
            if (inRange || InRange(location, targetLocation, maxRange, minRange, fullAirspace, isInAir, abstractType))
            {
                if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    Pointer<HouseClass> pTargetHouse = pTechno.Ref.Owner;
                    return pHouse.CanAffectHouse(pTargetHouse, owner, allied, enemies, civilian);
                }
                else if (pObject.CastToBullet(out Pointer<BulletClass> pBullet) && pBullet.TryGetStatus(out BulletStatusScript bulletStatus) && !bulletStatus.LifeData.IsDetonate)
                {
                    Pointer<HouseClass> pTargetHouse = IntPtr.Zero;
                    if (!pHouse.IsNull && !(pTargetHouse = bulletStatus.pSourceHouse).IsNull)
                    {
                        // 检查原始所属
                        return pHouse.CanAffectHouse(pTargetHouse, owner, allied, enemies, civilian);
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        private static bool InRange(CoordStruct location, CoordStruct targetLocation, double maxRange, double minRange, bool fullAirspace, bool isInAir, AbstractType abstractType)
        {
            double distance = location.DistanceFrom(targetLocation, fullAirspace);
            if (!fullAirspace && isInAir && abstractType == AbstractType.Aircraft)
            {
                distance *= 0.5;
            }
            return distance >= minRange && distance <= maxRange;
        }

        public static void FindTechno(this List<Pointer<TechnoClass>> array, Found<TechnoClass> func,
            CoordStruct location, double maxSpread, double minSpread = 0, bool fullAirspace = false,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            // 最大范围小于0，搜索全部，等于0，搜索1格范围
            double maxRange = (maxSpread > 0 ? maxSpread : maxSpread == 0 ? 1 : 0) * 256;
            double minRange = (minSpread <= 0 ? 0 : minSpread) * 256;
            for (int i = array.Count - 1; i >= 0; i--)
            {
                Pointer<TechnoClass> pTarget = array[i];
                // 分离类型
                if (!pTarget.IsDeadOrInvisible() && Hit(pTarget, location, maxRange, minRange, fullAirspace, pHouse, owner, allied, enemies, civilian))
                {
                    if (func(pTarget))
                    {
                        break;
                    }
                }
            }
        }

        private static bool Hit(Pointer<TechnoClass> pTechno,
            CoordStruct location, double maxRange, double minRange, bool fullAirspace,
            Pointer<HouseClass> pHouse,
            bool owner, bool allied, bool enemies, bool civilian)
        {
            bool inRange = maxRange == 0 || default == location;
            CoordStruct targetLocation = inRange ? default : pTechno.Ref.Base.Base.GetCoords(); // 不检查距离就不用算
            bool isInAir = false; // 全空域就不用算
            AbstractType abstractType = default; // 全空域就不用算
            if (!fullAirspace)
            {
                isInAir = pTechno.Ref.Base.Base.IsInAir();
                abstractType = pTechno.Ref.Base.Base.WhatAmI();
            }
            if (inRange || InRange(location, targetLocation, maxRange, minRange, fullAirspace, isInAir, abstractType))
            {
                Pointer<HouseClass> pTargetHouse = pTechno.Ref.Owner;
                return pHouse.CanAffectHouse(pTargetHouse, owner, allied, enemies, civilian);
            }
            return false;
        }

        public static void FindFoot(Found<FootClass> func,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            FindFoot(func, default, 0, 0, false, pHouse, owner, allied, enemies, civilian);
        }

        public static void FindFoot(Found<FootClass> func,
            CoordStruct location, double maxSpread, double minSpread, bool fullAirspace,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            InfantryClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, maxSpread, minSpread, fullAirspace, pHouse, owner, allied, enemies, civilian);
            UnitClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, maxSpread, minSpread, fullAirspace, pHouse, owner, allied, enemies, civilian);
            AircraftClass.Array.FindObject((pTarget) => { return func(pTarget.Convert<FootClass>()); }, location, maxSpread, minSpread, fullAirspace, pHouse, owner, allied, enemies, civilian);
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

        public static List<Pointer<TechnoClass>> GetCellSpreadTechnos(CoordStruct location, double spread, bool fullAirspace, bool includeInAir, bool ignoreBulidingOuter,
                    Pointer<HouseClass> pHouse = default,
                    bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            CellStruct currentCell = MapClass.Coord2Cell(location);
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCurretCell))
            {
                currentCell = pCurretCell.Ref.MapCoords;
            }
            return GetCellSpreadTechnos(currentCell, location, spread, fullAirspace, includeInAir, ignoreBulidingOuter, pHouse, owner, allied, enemies, civilian);
        }

        public static List<Pointer<TechnoClass>> GetCellSpreadTechnos(CellStruct current, CoordStruct location, double spread, bool fullAirspace, bool includeInAir, bool ignoreBulidingOuter,
            Pointer<HouseClass> pHouse = default,
            bool owner = true, bool allied = true, bool enemies = true, bool civilian = true)
        {
            HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();

            uint range = (uint)(spread + 0.99);
            CellSpreadEnumerator enumerator = new CellSpreadEnumerator(range);

            do
            {
                CellStruct offset = enumerator.Current;
                if (MapClass.Instance.TryGetCellAt(current + offset, out Pointer<CellClass> pCell))
                {
                    pCell.FindTechnoInCell((pTarget) =>
                    {
                        Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                        if (pHouse.CanAffectHouse(pTargetHouse, owner, allied, enemies, civilian))
                        {
                            pTechnoSet.Add(pTarget);
                        }
                        return false;
                    });
                }
                // 获取JJ
                if (includeInAir && !pCell.IsNull && !pCell.Ref.Jumpjet.IsNull)
                {
                    Pointer<TechnoClass> pJJ = pCell.Ref.Jumpjet.Convert<TechnoClass>();
                    Pointer<HouseClass> pTargetHouse = pJJ.Ref.Owner;
                    if (!pHouse.CanAffectHouse(pTargetHouse, owner, allied, enemies, civilian))
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
                }, location, spread, 0, false, pHouse, owner, allied, enemies, civilian);
            }
            // Logger.Log("includeAir = {0}, pTechnoSet.Count = {1}", includeInAir, pTechnoSet.Count);
            // 筛选并去掉不可用项目
            List<Pointer<TechnoClass>> pTechnoList = new List<Pointer<TechnoClass>>();
            foreach (Pointer<TechnoClass> pTechno in pTechnoSet)
            {
                CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                double dist = targetPos.DistanceFrom(location, fullAirspace);

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
        public static void FindTechnoOnMark(FoundAEM<TechnoClass> func,
            CoordStruct location, double maxSpread, double minSpread, bool fullAirspace,
            Pointer<HouseClass> pHouse, FilterData data, Pointer<ObjectClass> exclude)
        {
            List<Pointer<TechnoClass>> pTechnoList = null;
            if (maxSpread <= 0)
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
                pTechnoList = FinderHelper.GetCellSpreadTechnos(location, maxSpread, fullAirspace, data.AffectInAir, false, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
            }
            if (null != pTechnoList)
            {
                foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                {
                    // Logger.Log($"{Game.CurrentFrame} AffectSelf = {data.AffectSelf} && {pTarget} == {exclude} = {pTarget.Convert<ObjectClass>() == exclude}");
                    // 检查死亡
                    if (pTarget.IsDeadOrInvisible() || (!data.AffectSelf && pTarget.Convert<ObjectClass>() == exclude))
                    {
                        continue;
                    }
                    // 过滤替身和虚单位
                    if (pTarget.TryGetStatus(out var status) && (status.AmIStand() || status.VirtualUnit))
                    {
                        continue;
                    }
                    // 检查最小距离
                    if (minSpread > 0)
                    {
                        double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords(), fullAirspace);
                        if (!fullAirspace && pTarget.InAir() && pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                        {
                            distance *= 0.5;
                        }
                        if (distance < minSpread * 256)
                        {
                            continue;
                        }
                    }
                    // 可影响
                    if (data.CanAffectType(pTarget) && pTarget.TryGetAEManager(out AttachEffectScript aeManager) && data.IsOnMark(aeManager))
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
        public static void FindBulletOnMark(FoundAEM<BulletClass> func, CoordStruct location,
            double maxSpread, double minSpread, bool fullAirspace,
            Pointer<HouseClass> pHouse, FilterData data, Pointer<ObjectClass> exclude)
        {
            HashSet<Pointer<BulletClass>> pBulletSet = new HashSet<Pointer<BulletClass>>();
            BulletClass.Array.FindObject((pTarget) =>
            {
                if (!pTarget.IsDeadOrInvisible() && (data.AffectSelf || pTarget.Convert<ObjectClass>() != exclude))
                {
                    // 可影响
                    if (data.CanAffectType(pTarget))
                    {
                        pBulletSet.Add(pTarget);
                    }
                }
                return false;
            }, location, maxSpread, minSpread, fullAirspace, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);
            foreach (Pointer<BulletClass> pBullet in pBulletSet)
            {
                if (pBullet.TryGetAEManager(out AttachEffectScript aeManager) && data.IsOnMark(aeManager))
                {
                    // 执行动作
                    if (func(pBullet, aeManager))
                    {
                        break;
                    }
                }
            }
        }

    }
}
