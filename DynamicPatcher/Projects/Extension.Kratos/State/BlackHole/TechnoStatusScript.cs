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

    public partial class TechnoStatusScript : IBlackHoleVictim
    {

        public BlackHoleState BlackHoleState = new BlackHoleState();

        // 黑洞受害者
        private bool captureByBlackHole;
        private SwizzleablePointer<ObjectClass> pBlackHole = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);
        private BlackHoleData blackHoleData;
        private TimerStruct blackHoleDamageDelay;
        private bool lostControl;

        public void InitState_BlackHole()
        {
            // 初始化状态机
            BlackHoleData data = Ini.GetConfig<BlackHoleData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                BlackHoleState.Enable(data);
            }
        }

        public void OnUpdate_BlackHole()
        {
            if (!pTechno.IsInvisible())
            {
                // 黑洞吸人
                if (BlackHoleState.IsReady())
                {
                    BlackHoleState.StartCapture(pTechno.Convert<ObjectClass>(), pTechno.Ref.Owner);
                }
                // 被黑洞吸取中
                if (captureByBlackHole)
                {
                    if (pBlackHole.IsNull
                        || !pBlackHole.Pointer.TryGetBlackHoleState(out BlackHoleState blackHoleState)
                        || !blackHoleState.IsActive()
                        || OutOfBlackHole(blackHoleState)
                        || !blackHoleState.IsOnMark(pTechno.Convert<ObjectClass>())
                    )
                    {
                        CancelBlackHole();
                    }
                    else
                    {
                        Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
                        if (!IsBuilding)
                        {
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响，开始调整位置");
                            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                            // 从占据的格子中移除自己
                            pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                            // 停止移动
                            Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                            // LocomotionClass.ChangeLocomotorTo(pFoot, LocomotionClass.Jumpjet);
                            ILocomotion loco = pFoot.Ref.Locomotor;
                            loco.Stop_Moving();
                            loco.Mark_All_Occupation_Bits(0);
                            loco.Lock();
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 停止行动");
                            // 计算下一个坐标点
                            // 以偏移量为FLH获取目标点
                            CoordStruct targetPos = pBlackHole.Pointer.GetFLHAbsoluteCoords(blackHoleData.Offset, blackHoleData.IsOnTurret);
                            CoordStruct nextPos = targetPos;
                            double dist = targetPos.DistanceFrom(sourcePos);
                            // 获取捕获速度
                            int speed = blackHoleData.GetCaptureSpeed(pTechno.Ref.Type.Ref.Weight);
                            if (dist > speed)
                            {
                                // 计算下一个坐标
                                nextPos = FLHHelper.GetForwardCoords(sourcePos, targetPos, speed);
                            }
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 自身速度 {pTechno.Ref.Type.Ref.Speed} 捕获速度 {speed} 质量{pTechno.Ref.Type.Ref.Weight} 黑洞捕获速度 {blackHoleData.CaptureSpeed}");
                            int deltaZ = sourcePos.Z - targetPos.Z;
                            bool canMove = true;
                            // 检查地面
                            if (MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pTargetCell))
                            {
                                CoordStruct cellPos = pTargetCell.Ref.GetCoordsWithBridge();
                                if (cellPos.Z > nextPos.Z)
                                {
                                    // 沉入地面
                                    nextPos.Z = cellPos.Z;
                                    // 检查悬崖
                                    switch (pTargetCell.Ref.GetTileType())
                                    {
                                        case TileType.Cliff:
                                        case TileType.DestroyableCliff:
                                            // 悬崖上可以往悬崖下移动
                                            canMove = deltaZ > 0;
                                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到悬崖 {(canMove ? "可通过" : "不可通过")} nextPos = {nextPos}");
                                            break;
                                    }
                                }
                                // 检查建筑
                                // 会飞的单位不检查建筑
                                if (!pTechno.Ref.Type.Ref.ConsideredAircraft && !blackHoleData.AllowPassBuilding)
                                {
                                    Pointer<BuildingClass> pBuilding = pTargetCell.Ref.GetBuilding();
                                    if (!pBuilding.IsNull)
                                    {
                                        canMove = !pBuilding.CanHit(nextPos.Z);
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到建筑 [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] {pBuilding} {(canMove ? "可通过" : "不可通过")} nextPos {nextPos}");
                                    }
                                }

                            }
                            if (!canMove)
                            {
                                // 反弹回移动前的格子
                                if (MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pSourceCell))
                                {
                                    CoordStruct cellPos = pSourceCell.Ref.GetCoordsWithBridge();
                                    nextPos.X = cellPos.X;
                                    nextPos.Y = cellPos.Y;
                                    if (nextPos.Z < cellPos.Z)
                                    {
                                        nextPos.Z = cellPos.Z;
                                    }
                                }
                            }

                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 获得新位置坐标 {nextPos} 原始位置 {sourcePos} {(!canMove ? "受到阻挡不能前进，返回" : "")}");
                            // 被黑洞吸走
                            pTechno.Ref.Base.Mark(MarkType.UP);
                            pTechno.Ref.Base.SetLocation(nextPos);
                            pTechno.Ref.Base.Mark(MarkType.DOWN);
                            // 设置动作
                            if (blackHoleData.AllowCrawl && pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Infantry)
                            {
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置步兵匍匐动作");
                                pFoot.Ref.Inf_PlayAnim(SequenceAnimType.CRAWL);
                            }
                            // 设置翻滚
                            if (blackHoleData.AllowRotateUnit)
                            {
                                if (pTechno.Ref.IsVoxel() && canMove)
                                {
                                    // pTechno.Ref.RockingForwardsPerFrame = 0.2f;
                                    // pTechno.Ref.RockingSidewaysPerFrame = 0.2f;
                                }
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置VXL翻滚动作");
                                // 设置朝向
                                if (lastMission == Mission.Move || lastMission == Mission.AttackMove || pTechno.Ref.Type.Ref.ConsideredAircraft || !pTechno.InAir())
                                {
                                    DirStruct facingDir = FLHHelper.Point2Dir(targetPos, sourcePos);
                                    pTechno.Ref.Facing.turn(facingDir);
                                    Guid locoId = loco.ToLocomotionClass().Ref.GetClassID();
                                    if (locoId == LocomotionClass.Jumpjet)
                                    {
                                        // JJ朝向是单独的Facing
                                        Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                                        pLoco.Ref.LocomotionFacing.turn(facingDir);
                                    }
                                    else if (locoId == LocomotionClass.Fly)
                                    {
                                        // 飞机使用的炮塔的Facing
                                        pTechno.Ref.TurretFacing.turn(facingDir);
                                    }
                                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 扭头，屁股朝前");
                                }
                            }
                        }
                        if (null != blackHoleData)
                        {
                            // 黑洞伤害
                            if (blackHoleData.AllowDamageTechno && blackHoleData.Damage != 0 && !BlackHoleState.IsActive())
                            {
                                if (blackHoleDamageDelay.Expired())
                                {
                                    blackHoleDamageDelay.Start(blackHoleData.DamageDelay);
                                    // Logger.Log($"{Game.CurrentFrame} 黑洞对 [{section}]{pTechno} 造成伤害 准备中 Damage = {blackHoleData.Damage}, ROF = {blackHoleData.DamageDelay}, WH = {blackHoleData.DamageWH}");
                                    Pointer<WarheadTypeClass> pWH = RulesClass.Global().C4Warhead;
                                    if (!blackHoleData.DamageWH.IsNullOrEmptyOrNone())
                                    {
                                        pWH = WarheadTypeClass.ABSTRACTTYPE_ARRAY.Find(blackHoleData.DamageWH);
                                    }
                                    if (!pWH.IsNull)
                                    {
                                        Pointer<ObjectClass> pAttacker = IntPtr.Zero;
                                        Pointer<HouseClass> pAttackingHouse = IntPtr.Zero;
                                        if (pBlackHole.Pointer.CastToBullet(out Pointer<BulletClass> pBullet))
                                        {
                                            pAttacker = pBullet.Ref.Owner.Convert<ObjectClass>();
                                            pAttackingHouse = pBullet.GetSourceHouse();
                                        }
                                        else
                                        {
                                            pAttacker = pBlackHole;
                                            pAttackingHouse = pBlackHole.Pointer.Convert<TechnoClass>().Ref.Owner;
                                        }
                                        // Logger.Log($"{Game.CurrentFrame} 黑洞对 [{section}]{pTechno} 造成伤害 Damage = {blackHoleData.Damage}, ROF = {blackHoleData.DamageDelay}, WH = {pWH.Ref.Base.ID}");
                                        pTechno.Ref.Base.TakeDamage(blackHoleData.Damage, pWH, pAttacker, pAttackingHouse, pTechno.Ref.Type.Ref.Crewed);
                                    }
                                }
                            }
                            // 目标设置
                            if (blackHoleData.ClearTarget)
                            {
                                ClearTarget();
                            }
                            if (blackHoleData.ChangeTarget)
                            {
                                pTechno.Ref.SetTarget(pBlackHole.Pointer.Convert<AbstractClass>());
                            }
                            // 失控设置
                            if (!IsBuilding && blackHoleData.OutOfControl)
                            {
                                lostControl = true;
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 失去控制");
                                ClearTarget();
                                pTechno.Ref.Base.Deselect();
                                pMission.Ref.ForceMission(Mission.None);
                                pMission.Ref.QueueMission(Mission.Sleep, false);
                                // pTechno.Convert<FootClass>().Ref.IsAttackedByLocomotor = true;
                            }
                        }
                    }
                }
            }

        }

        public void SetBlackHole(Pointer<ObjectClass> pBlackHole, BlackHoleData blackHoleData)
        {
            if (!this.captureByBlackHole || null == this.blackHoleData || this.blackHoleData.Weight < blackHoleData.Weight || ((this.blackHoleData.Weight == blackHoleData.Weight || blackHoleData.Weight <= 0) && blackHoleData.CaptureFromSameWeight))
            {
                this.pBlackHole.Pointer = pBlackHole;
                this.blackHoleData = blackHoleData;
                this.captureByBlackHole = true;
            }
        }

        public void CancelBlackHole()
        {
            // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pTechno} 不再受 黑洞 {pBlackHole.Pointer} 的影响");
            if (captureByBlackHole && !IsBuilding && !pTechno.IsDeadOrInvisible())
            {
                Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
                Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                // LocomotionClass.ChangeLocomotorTo(pFoot, pTechno.Ref.Type.Ref.Locomotor);
                pFoot.Ref.Locomotor.Unlock();
                // 恢复可控制
                if (lostControl)
                {
                    pMission.Ref.ForceMission(Mission.Guard);
                    // pTechno.Convert<FootClass>().Ref.IsAttackedByLocomotor = false;
                }
                // 停止转身
                if (blackHoleData.AllowRotateUnit)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置VXL翻滚动作");
                    FacingStruct facing = pTechno.Ref.Facing;
                    ILocomotion loco = pFoot.Ref.Locomotor;
                    Guid locoId = loco.ToLocomotionClass().Ref.GetClassID();
                    if (locoId == LocomotionClass.Jumpjet)
                    {
                        // JJ朝向是单独的Facing
                        Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                        facing = pLoco.Ref.LocomotionFacing;
                    }
                    else if (locoId == LocomotionClass.Fly)
                    {
                        // 飞机使用的炮塔的Facing
                        facing = pTechno.Ref.TurretFacing;
                    }
                    if (facing.in_motion())
                    {
                        facing.set(facing.current());
                    }
                }
                // 检查是否在悬崖上摔死
                bool canPass = true;
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                CoordStruct targetPos = location;
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    targetPos = pCell.Ref.GetCoordsWithBridge();
                    // 当前格子所在的位置不可通行，炸了它
                    canPass = pCell.Ref.IsClearToMove(pTechno.Ref.Type.Ref.SpeedType, pTechno.Ref.Type.Ref.MovementZone, true, true);
                    if (pTechno.Ref.Base.GetHeight() < 0)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  位于地下 {pTechno.Ref.Base.GetHeight()}，调整回地表");
                        pTechno.Ref.Base.SetLocation(targetPos);
                    }
                }
                int height = pTechno.Ref.Base.GetHeight();
                if (pTechno.Ref.Type.Ref.ConsideredAircraft && height > Game.LevelHeight * 2)
                {
                    // 飞行器在天上，免死
                    // Logger.Log($"{Game.CurrentFrame} 飞机 恢复行动");
                    pTechno.Ref.SetDestination(pCell);
                    if (pTechno.Ref.Target.IsNull)
                    {
                        pMission.Ref.QueueMission(Mission.Guard, false);
                    }
                    else
                    {
                        pMission.Ref.QueueMission(Mission.Attack, false);
                    }
                }
                else
                {
                    bool drop = false;
                    if (null != blackHoleData)
                    {
                        // 超过高度也摔死
                        if (blackHoleData.AllowFallingDestroy && height >= blackHoleData.FallingDestroyHeight)
                        {
                            canPass = false;
                        }
                    }
                    if (canPass)
                    {
                        pTechno.Ref.Base.MarkAllOccupationBits(targetPos);
                        if (height > 0)
                        {
                            // 离地
                            pTechno.Ref.Base.IsFallingDown = true;
                            drop = true;
                        }
                        else
                        {
                            // 贴地
                            pTechno.Ref.Base.Scatter(targetPos, true, true);
                        }
                    }
                    else
                    {
                        // 摔死
                        // Logger.Log($"{Game.CurrentFrame} {(IsBuilding ? "建筑" : "单位")} [{section}] {pTechno} 下方不可通行 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        pTechno.Ref.Base.DropAsBomb();
                        drop = true;
                    }
                    if (drop && pTechno.CastToInfantry(out Pointer<InfantryClass> pInf))
                    {
                        pInf.Ref.Base.Inf_PlayAnim(SequenceAnimType.Paradrop);
                    }
                }
            }
            this.captureByBlackHole = false;
            this.blackHoleData = null;
            this.lostControl = false;
            this.pBlackHole.Pointer = IntPtr.Zero;
        }

        private bool OutOfBlackHole(BlackHoleState blackHoleState)
        {
            CoordStruct taregtPos = pBlackHole.Ref.Base.GetCoords();
            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
            return blackHoleState.IsOutOfRange(taregtPos.DistanceFrom(sourcePos));
        }

    }
}
