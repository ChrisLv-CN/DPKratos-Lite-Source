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

    [Serializable]
    [GlobalScriptable(typeof(BulletExt))]
    [UpdateAfter(typeof(BulletStatusScript))]
    public class ProximityScript : BulletScriptable
    {
        public ProximityScript(BulletExt owner) : base(owner) { }

        private BulletStatusScript bulletStatus => GameObject.GetComponent<BulletStatusScript>();

        private ProximityData proximityData => Ini.GetConfig<ProximityData>(Ini.RulesDependency, section).Data;

        private Proximity proximity;
        private int proximityRange = -1;

        public override void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType)
        {
            // 设置提前引爆抛射体
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
            // 设置碰触引擎
            if (proximityData.Force)
            {
                ActiveProximity();
            }
        }

        public void ActiveProximity()
        {
            this.proximity = new Proximity(pBullet.Ref.Type.Ref.CourseLockDuration, proximityData.PenetrationTimes);
        }

        public override void OnLateUpdate()
        {
            if (!pBullet.IsDeadOrInvisible())
            {
                CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                // 提前引爆抛射体
                if (proximityRange >= 0)
                {
                    if (sourcePos.DistanceFrom(pBullet.Ref.TargetCoords) <= proximityRange)
                    {
                        ManualDetonation(sourcePos);
                    }
                }

                // 计算碰触引信
                if (null != proximity && !proximity.IsSafe())
                {
                    // 读取预定目标格子上的建筑
                    Pointer<CellClass> pSourceTargetCell = IntPtr.Zero;
                    Pointer<BuildingClass> pSourceTargetBuilding = IntPtr.Zero;
                    if (MapClass.Instance.TryGetCellAt(pBullet.Ref.TargetCoords, out pSourceTargetCell))
                    {
                        pSourceTargetBuilding = pSourceTargetCell.Ref.GetBuilding();
                        // Logger.Log($"{Game.CurrentFrame} 导弹预定目标格子中有建筑 {pSourceTargetBuilding}");
                    }
                    // 当前所处的位置距离预定飞行目标过近，且在同一格内，跳过碰撞检测
                    if (sourcePos.DistanceFrom(pBullet.Ref.TargetCoords) <= 256
                        && MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pSourceCell)
                        && pSourceCell == pSourceTargetCell)
                    {
                        return;
                    }
                    // 计算碰撞的半径，超过1格，确定搜索范围
                    int cellSpread = (proximityData.Arm / 256) + 1;
                    // Logger.Log("Arm = {0}，确定搜索范围 {1} 格", Proximity.Data.Arm, cellSpread);

                    // 每个格子只检查一次
                    if (MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pCell) && pCell != proximity.pCheckedCell)
                    {
                        proximity.pCheckedCell.Pointer = pCell;
                        CoordStruct cellPos = pCell.Ref.Base.GetCoords();

                        // BulletEffectHelper.GreenCell(cellPos, 128, 1, 75);

                        // 获取这个格子上的所有对象
                        HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();

                        // 搜索范围加大一格，搜索需要排除四周格子上的建筑，只获取当前格子上找到的建筑
                        // 获取范围内的所有对象，不包括飞行器
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
                                    // 检查死亡和发射者和替身
                                    if (!IsDeadOrStand(pTarget))
                                    {
                                        // 过滤周边格子上的建筑，但获取当前格子上的建筑
                                        if (pTarget.Ref.Base.Base.WhatAmI() != AbstractType.Building || pCheckCell == pCell)
                                        {
                                            // Logger.Log($"{Game.CurrentFrame} 检索到当前格子的对象[{pTarget.Ref.Type.Ref.Base.Base.ID}]，加入列表");
                                            pTechnoSet.Add(pTarget);
                                        }
                                    }
                                    return false;
                                });
                                // 获取JJ
                                Pointer<TechnoClass> pJJ = pCheckCell.Ref.Jumpjet.Convert<TechnoClass>();
                                if (!IsDeadOrStand(pJJ))
                                {
                                    // Logger.Log($"{Game.CurrentFrame} 检索到当前格子的JJ [{pJJ.Ref.Type.Ref.Base.Base.ID}]，加入列表");
                                    pTechnoSet.Add(pJJ);
                                }
                            }
                        } while (enumerator.MoveNext());

                        // 获取所有在天上的玩意儿，飞起来的步兵坦克，包含路过的飞机
                        ExHelper.FindFoot((pTarget) => {
                            Pointer<TechnoClass> pTechno = pTarget.Convert<TechnoClass>();
                            if (!IsDeadOrStand(pTechno) && pTechno.Ref.Base.GetHeight() > 0)
                            {
                                // 消去高度差，检查和当前坐标距离在cellSpread格范围内
                                CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                                targetPos.Z = cellPos.Z;
                                if (targetPos.DistanceFrom(cellPos) <= cellSpread * 256)
                                {
                                    // Logger.Log($"{Game.CurrentFrame} 检索到当前范围{cellSpread}格内的坦克 [{pTechno.Ref.Type.Ref.Base.Base.ID}]，加入列表");
                                    pTechnoSet.Add(pTechno);
                                }
                            }
                            return false;
                        });

                        // 筛选并处理找到的对象
                        foreach (Pointer<TechnoClass> pTarget in pTechnoSet)
                        {
                            if (pTarget.IsDeadOrInvisible())
                            {
                                continue;
                            }
                            CoordStruct targetPos = pTarget.Ref.Base.Base.GetCoords();
                            // BulletEffectHelper.BlueLineZ(targetPos, 1024, 1, 75);

                            bool hit = false; // 无视高度和距离，格子内的对象都算碰撞目标

                            if (pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                            {
                                // 检查建筑在范围内
                                Pointer<BuildingClass> pBuilding = pTarget.Convert<BuildingClass>();
                                hit = pBuilding.CanHit(sourcePos.Z, proximityData.Blade, proximityData.ZOffset);
                                // Logger.Log($"{Game.CurrentFrame} 碰触建筑 {pBuilding}");
                                // 检查建筑是否被炸过
                                if (hit && proximityData.PenetrationBuildingOnce)
                                {
                                    hit = pBuilding != pSourceTargetBuilding && !proximity.CheckAndMarkBuilding(pBuilding);
                                }
                            }
                            else
                            {
                                // 使用抛射体所经过的格子的中心点作为判定原点
                                CoordStruct sourceTestPos = cellPos;
                                // 判定原点抬升至与抛射体同高
                                sourceTestPos.Z = sourcePos.Z;
                                // 目标点在脚下，加上高度修正偏移值
                                CoordStruct targetTestPos = targetPos + new CoordStruct(0, 0, proximityData.ZOffset);
                                if (proximityData.Blade)
                                {
                                    // 无视高度，只检查横向距离
                                    targetTestPos.Z = sourceTestPos.Z;
                                }
                                // BulletEffectHelper.RedCrosshair(sourceTestPos, 128, 1, 75);
                                // BulletEffectHelper.RedCrosshair(targetTestPos, 128, 1, 75);
                                // BulletEffectHelper.BlueLine(sourceTestPos, targetTestPos, 3, 75);
                                hit = targetTestPos.DistanceFrom(sourceTestPos) <= proximityData.Arm;
                                // Logger.Log("目标单位坐标加上修正值{0}, 与抛射体的距离{1}，检测半径{2}", Proximity.Data.ZOffset, targetTestPos.DistanceFrom(sourceTestPos), Proximity.Data.Arm);
                            }

                            // 敌我识别并引爆
                            if (hit && AffectTarget(pTarget))
                            {
                                // 引爆
                                CoordStruct detonatePos = targetPos; // 爆点在与目标位置
                                // BulletEffectHelper.RedCrosshair(detonatePos, 2048, 1, 75);
                                // Logger.Log("抛射体位置{0}，偏移位置{1}", sourcePos, detonatePos);
                                if (ManualDetonation(sourcePos, !proximityData.Penetration, pTarget.Convert<AbstractClass>(), detonatePos))
                                {
                                    // 爆了就结束了
                                    break;
                                }
                            }

                        }
                    }
                }
            }
        }

        private bool IsDeadOrStand(Pointer<TechnoClass> pTarget)
        {
            // 检查死亡和发射者
            if (pTarget.IsDeadOrInvisible() || pTarget == pBullet.Ref.Owner
                || (!proximityData.AffectsClocked && pTarget.IsCloaked())
                || pTarget.Ref.IsImmobilized)
            {
                return true;
            }
            // 过滤掉替身
            return pTarget.AmIStand();
        }

        private bool AffectTarget(Pointer<TechnoClass> pTarget)
        {
            Pointer<HouseClass> pTargetOwner = IntPtr.Zero;
            if (!pTarget.IsNull && !(pTargetOwner = pTarget.Ref.Owner).IsNull)
            {
                if (pTargetOwner == bulletStatus.pSourceHouse)
                {
                    return proximityData.AffectsAllies || proximityData.AffectsOwner;
                }
                else if (pTargetOwner.Ref.IsAlliedWith(bulletStatus.pSourceHouse))
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
            //     // 发射者死亡，抛射体跟着一起玩完
            //     this.proximity = null;
            //     KABOOM = true;
            // }

            // 检查穿透次数是否用完
            KABOOM = KABOOM || null == proximity || !proximityData.Penetration || proximity.TimesDone();

            if (KABOOM)
            {
                // 抛射体原地爆炸
                pBullet.Ref.Detonate(sourcePos);
                pBullet.Ref.Base.Remove();
                pBullet.Ref.Base.UnInit();
            }
            else if (!pTarget.IsNull)
            {
                // 抛射体原地爆炸，但不销毁，并且启用自定义武器，自定义弹头
                if (default == detonatePos)
                {
                    detonatePos = sourcePos;
                }

                // 使用自身制造伤害
                int damage = pBullet.Ref.Base.Health;
                Pointer<WarheadTypeClass> pWH = pBullet.Ref.WH;

                // 检查自定义武器是否存在，存在则使用自定义武器制造伤害，不存在就用自身制造伤害
                string weaponId = proximityData.PenetrationWeapon;
                if (!string.IsNullOrEmpty(weaponId))
                {
                    // 对敌人造成自定义武器的伤害
                    Pointer<WeaponTypeClass> pWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponId);
                    if (!pWeapon.IsNull)
                    {
                        damage = pWeapon.Ref.Damage;
                        pWH = pWeapon.Ref.Warhead;
                    }
                }
                // 检查是否使用其他弹头
                string warheadId = proximityData.PenetrationWarhead;
                if (!string.IsNullOrEmpty(warheadId))
                {
                    Pointer<WarheadTypeClass> pOverrideWH = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(warheadId);
                    if (!pOverrideWH.IsNull)
                    {
                        pWH = pOverrideWH;
                    }
                }

                // 在预定引爆地点引爆弹头
                MapClass.DamageArea(detonatePos, damage, pBullet.Ref.Owner, pWH, pWH.Ref.Tiberium, bulletStatus.pSourceHouse);
                // 播放弹头动画
                LandType landType = proximity.pCheckedCell.IsNull ? LandType.Clear : proximity.pCheckedCell.Ref.LandType;
                Pointer<AnimTypeClass> pAnimType = MapClass.SelectDamageAnimation(damage, pWH, landType, sourcePos);
                if (!pAnimType.IsNull)
                {
                    Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                    pAnim.Ref.Owner = bulletStatus.pSourceHouse;
                }
                // 计数器减1
                proximity.ThroughOnce();
            }
            return KABOOM;
        }
    }
}
