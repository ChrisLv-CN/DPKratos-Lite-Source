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

    [Serializable]
    public class Proximity
    {
        public SwizzleablePointer<CellClass> pCheckedCell;
        public List<SwizzleablePointer<BuildingClass>> BuildingMarks;

        private bool count;
        private int times;

        private bool safe;
        private TimerStruct safeTimer;

        public Proximity(int safeDelay, int times)
        {
            this.pCheckedCell = new SwizzleablePointer<CellClass>(IntPtr.Zero);
            this.BuildingMarks = new List<SwizzleablePointer<BuildingClass>>();

            this.safe = safeDelay > 0;
            this.safeTimer.Start(safeDelay);
            this.count = times > 0;
            this.times = times;
        }

        public bool IsSafe()
        {
            if (safe)
            {
                safe = safeTimer.InProgress();
            }
            return safe;
        }

        public void ThroughOnce()
        {
            if (count)
            {
                times--;
            }
        }

        public bool TimesDone()
        {
            return count && times <= 0;
        }

        public bool CheckAndMarkBuilding(Pointer<BuildingClass> pBuilding)
        {
            bool find = false;
            for (int i = 0; i < BuildingMarks.Count; i++)
            {
                SwizzleablePointer<BuildingClass> pMark = BuildingMarks[i];
                if (pBuilding == pMark)
                {
                    find = true;
                    break;
                }
            }
            if (!find)
            {
                SwizzleablePointer<BuildingClass> mark = new SwizzleablePointer<BuildingClass>(pBuilding);
                BuildingMarks.Add(mark);
            }
            return find;
        }
    }


    public partial class BulletStatusScript
    {

        private IConfigWrapper<ProximityData> _proximityData;
        private ProximityData proximityData
        {
            get
            {
                if (null == _proximityData)
                {
                    _proximityData = Ini.GetConfig<ProximityData>(Ini.RulesDependency, section);
                }
                return _proximityData.Data;
            }
        }

        private Proximity proximity; // ?????????????????????
        private int proximityRange = -1;

        public void InitState_Proximity()
        {
            // ???????????????????????????
            Pointer<WeaponTypeClass> pWeapon = pBullet.Ref.WeaponType;
            if (!pWeapon.IsNull)
            {
                string weaponId = pWeapon.Ref.Base.ID;
                ProximityRangeData tempData = Ini.GetConfig<ProximityRangeData>(Ini.RulesDependency, weaponId).Data;
                int range = tempData.Range;
                if (tempData.Random)
                {
                    range = MathEx.Random.Next(tempData.MinRange, tempData.MaxRange);
                }
                if (range >= 0)
                {
                    this.proximityRange = range;
                }
            }
            // ??????????????????
            if (proximityData.Force)
            {
                ActiveProximity();
            }
        }

        public void ActiveProximity()
        {
            this.proximity = new Proximity(pBullet.Ref.Type.Ref.CourseLockDuration, proximityData.PenetrationTimes);
        }

        public void OnLateUpdate_Proximity(ref CoordStruct sourcePos)
        {

            // ?????????????????????
            if (proximityRange >= 0)
            {
                if (sourcePos.DistanceFrom(pBullet.Ref.TargetCoords) <= proximityRange)
                {
                    ManualDetonation(sourcePos);
                }
            }

            // ??????????????????
            if (null != proximity && !proximity.IsSafe())
            {
                // ????????????????????????????????????
                Pointer<CellClass> pSourceTargetCell = IntPtr.Zero;
                Pointer<BuildingClass> pSourceTargetBuilding = IntPtr.Zero;
                if (MapClass.Instance.TryGetCellAt(pBullet.Ref.TargetCoords, out pSourceTargetCell))
                {
                    pSourceTargetBuilding = pSourceTargetCell.Ref.GetBuilding();
                    // Logger.Log($"{Game.CurrentFrame} ???????????????????????????????????? {pSourceTargetBuilding}");
                }
                // ?????????????????????????????????????????????????????????????????????????????????????????????
                if (sourcePos.DistanceFrom(pBullet.Ref.TargetCoords) <= 256
                    && MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pSourceCell)
                    && pSourceCell == pSourceTargetCell)
                {
                    return;
                }
                // ??????????????????????????????1????????????????????????
                int cellSpread = (proximityData.Arm / 256) + 1;
                // Logger.Log("Arm = {0}????????????????????? {1} ???", Proximity.Data.Arm, cellSpread);

                // ???????????????????????????
                if (MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pCell) && pCell != proximity.pCheckedCell)
                {
                    proximity.pCheckedCell.Pointer = pCell;
                    CoordStruct cellPos = pCell.Ref.Base.GetCoords();

                    // BulletEffectHelper.GreenCell(cellPos, 128, 1, 75);

                    // ????????????????????????????????????
                    HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();

                    // ???????????????????????????????????????????????????????????????????????????????????????????????????????????????
                    // ???????????????????????????????????????????????????
                    CellSpreadEnumerator enumerator = new CellSpreadEnumerator((uint)cellSpread);
                    do
                    {
                        CellStruct cur = pCell.Ref.MapCoords;
                        CellStruct offset = enumerator.Current;
                        if (MapClass.Instance.TryGetCellAt(cur + offset, out Pointer<CellClass> pCheckCell))
                        {
                            // BulletEffectHelper.RedCell(pCheckCell.Ref.Base.GetCoords(), 128, 1, 30);
                            pCheckCell.FindTechnoInCell((pTarget) =>
                            {
                                // ?????????????????????????????????
                                if (!IsDeadOrStand(pTarget))
                                {
                                    // ??????????????????????????????????????????????????????????????????
                                    if (pTarget.Ref.Base.Base.WhatAmI() != AbstractType.Building || pCheckCell == pCell)
                                    {
                                        // Logger.Log($"{Game.CurrentFrame} ??????????????????????????????[{pTarget.Ref.Type.Ref.Base.Base.ID}]???????????????");
                                        pTechnoSet.Add(pTarget);
                                    }
                                }
                                return false;
                            });
                            // ??????JJ
                            Pointer<TechnoClass> pJJ = pCheckCell.Ref.Jumpjet.Convert<TechnoClass>();
                            if (!IsDeadOrStand(pJJ))
                            {
                                // Logger.Log($"{Game.CurrentFrame} ????????????????????????JJ [{pJJ.Ref.Type.Ref.Base.Base.ID}]???????????????");
                                pTechnoSet.Add(pJJ);
                            }
                        }
                    } while (enumerator.MoveNext());

                    // ????????????????????????????????????????????????????????????????????????????????????
                    FinderHelper.FindFoot((pTarget) =>
                    {
                        Pointer<TechnoClass> pTechno = pTarget.Convert<TechnoClass>();
                        if (!IsDeadOrStand(pTechno) && pTechno.Ref.Base.GetHeight() > 0)
                        {
                            // ????????????????????????????????????????????????cellSpread????????????
                            CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                            targetPos.Z = cellPos.Z;
                            if (targetPos.DistanceFrom(cellPos) <= cellSpread * 256)
                            {
                                // Logger.Log($"{Game.CurrentFrame} ?????????????????????{cellSpread}??????????????? [{pTechno.Ref.Type.Ref.Base.Base.ID}]???????????????");
                                pTechnoSet.Add(pTechno);
                            }
                        }
                        return false;
                    });

                    // ??????????????????????????????
                    foreach (Pointer<TechnoClass> pTarget in pTechnoSet)
                    {
                        if (pTarget.IsDeadOrInvisible())
                        {
                            continue;
                        }
                        CoordStruct targetPos = pTarget.Ref.Base.Base.GetCoords();
                        // BulletEffectHelper.BlueLineZ(targetPos, 1024, 1, 75);

                        bool hit = false; // ????????????????????????????????????????????????????????????

                        if (pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                        {
                            // ????????????????????????
                            Pointer<BuildingClass> pBuilding = pTarget.Convert<BuildingClass>();
                            hit = pBuilding.CanHit(sourcePos.Z, proximityData.Blade, proximityData.ZOffset);
                            // Logger.Log($"{Game.CurrentFrame} ???????????? {pBuilding}");
                            // ???????????????????????????
                            if (hit && proximityData.PenetrationBuildingOnce)
                            {
                                hit = pBuilding != pSourceTargetBuilding && !proximity.CheckAndMarkBuilding(pBuilding);
                            }
                        }
                        else
                        {
                            // ???????????????????????????????????????????????????????????????
                            CoordStruct sourceTestPos = cellPos;
                            // ???????????????????????????????????????
                            sourceTestPos.Z = sourcePos.Z;
                            // ????????????????????????????????????????????????
                            CoordStruct targetTestPos = targetPos + new CoordStruct(0, 0, proximityData.ZOffset);
                            if (proximityData.Blade)
                            {
                                // ????????????????????????????????????
                                targetTestPos.Z = sourceTestPos.Z;
                            }
                            // BulletEffectHelper.RedCrosshair(sourceTestPos, 128, 1, 75);
                            // BulletEffectHelper.RedCrosshair(targetTestPos, 128, 1, 75);
                            // BulletEffectHelper.BlueLine(sourceTestPos, targetTestPos, 3, 75);
                            hit = targetTestPos.DistanceFrom(sourceTestPos) <= proximityData.Arm;
                            // Logger.Log("?????????????????????????????????{0}, ?????????????????????{1}???????????????{2}", Proximity.Data.ZOffset, targetTestPos.DistanceFrom(sourceTestPos), Proximity.Data.Arm);
                        }

                        // ?????????????????????
                        if (hit && AffectTarget(pTarget))
                        {
                            // ??????
                            CoordStruct detonatePos = targetPos; // ????????????????????????
                            if (ManualDetonation(sourcePos, !proximityData.Penetration, pTarget.Convert<AbstractClass>(), detonatePos))
                            {
                                // ??????????????????
                                break;
                            }
                        }

                    }
                }
            }
        }

        private bool IsDeadOrStand(Pointer<TechnoClass> pTarget)
        {
            // ????????????????????????
            if (pTarget.IsDeadOrInvisible() || pTarget == pBullet.Ref.Owner
                || (!proximityData.AffectsClocked && pTarget.IsCloaked())
                || pTarget.Ref.IsImmobilized)
            {
                return true;
            }
            // ???????????????
            return pTarget.AmIStand();
        }

        private bool AffectTarget(Pointer<TechnoClass> pTarget)
        {
            Pointer<HouseClass> pTargetOwner = IntPtr.Zero;
            if (!pTarget.IsNull && !(pTargetOwner = pTarget.Ref.Owner).IsNull)
            {
                if (pTargetOwner == pSourceHouse)
                {
                    return proximityData.AffectsAllies || proximityData.AffectsOwner;
                }
                else if (pTargetOwner.Ref.IsAlliedWith(pSourceHouse))
                {
                    return proximityData.AffectsAllies;
                }
                else
                {
                    return proximityData.AffectsEnemies;
                }
            }
            return false;
        }

        private bool ManualDetonation(CoordStruct sourcePos, bool KABOOM = true, Pointer<AbstractClass> pTarget = default, CoordStruct detonatePos = default)
        {
            // if (!KABOOM && (bulletStatus.pSourceShooter.IsNull || bulletStatus.pSourceShooter.Pointer.IsDead()))
            // {
            //     // ?????????????????????????????????????????????
            //     this.proximity = null;
            //     KABOOM = true;
            // }

            // ??????????????????????????????
            KABOOM = KABOOM || null == proximity || !proximityData.Penetration || proximity.TimesDone();

            if (KABOOM)
            {
                // ?????????????????????
                pBullet.Ref.Detonate(sourcePos);
                pBullet.Ref.Base.Remove();
                pBullet.Ref.Base.UnInit();
            }
            else if (!pTarget.IsNull)
            {
                // ????????????????????????????????????????????????????????????????????????????????????
                if (default == detonatePos)
                {
                    detonatePos = sourcePos;
                }

                // ????????????????????????
                int damage = pBullet.Ref.Base.Health;
                Pointer<WarheadTypeClass> pWH = pBullet.Ref.WH;

                // ??????????????????????????????????????????????????????????????????????????????????????????????????????????????????
                string weaponId = proximityData.PenetrationWeapon;
                if (!string.IsNullOrEmpty(weaponId))
                {
                    // ???????????????????????????????????????
                    Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                    if (!pWeapon.IsNull)
                    {
                        damage = pWeapon.Ref.Damage;
                        pWH = pWeapon.Ref.Warhead;
                    }
                }
                // ??????????????????????????????
                string warheadId = proximityData.PenetrationWarhead;
                if (!string.IsNullOrEmpty(warheadId))
                {
                    Pointer<WarheadTypeClass> pOverrideWH = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(warheadId);
                    if (!pOverrideWH.IsNull)
                    {
                        pWH = pOverrideWH;
                    }
                }

                // ?????????????????????????????????
                MapClass.DamageArea(detonatePos, damage, pBullet.Ref.Owner, pWH, pWH.Ref.Tiberium, pSourceHouse);
                // ??????????????????
                LandType landType = proximity.pCheckedCell.IsNull ? LandType.Clear : proximity.pCheckedCell.Ref.LandType;
                Pointer<AnimTypeClass> pAnimType = MapClass.SelectDamageAnimation(damage, pWH, landType, sourcePos);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                    pAnim.Ref.Owner = pSourceHouse;
                }
                // ????????????1
                proximity.ThroughOnce();
            }
            return KABOOM;
        }
    }
}
