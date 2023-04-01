using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Components;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript : IBlackHoleVictim
    {

        public BlackHoleState BlackHoleState = new BlackHoleState();

        // 黑洞受害者
        public bool CaptureByBlackHole;
        private IExtension blackHoleExt;
        private Pointer<ObjectClass> pBlackHole
        {
            get
            {
                if (null != blackHoleExt)
                {
                    return blackHoleExt.OwnerObject.Convert<ObjectClass>();
                }
                return default;
            }
        }

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
                BlackHoleState.Owner = Owner;
            }
        }

        public void OnUpdate_BlackHole()
        {
            if (!pTechno.IsInvisible())
            {
                // 黑洞吸人
                if (BlackHoleState.IsReady())
                {
                    BlackHoleState.StartCapture(Owner, pTechno.Ref.Owner);
                }
                // if (!isBuilding)
                // {
                //     ILocomotion locomotion = pTechno.Convert<FootClass>().Ref.Locomotor;
                //     if (locomotion.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Drive)
                //     {
                //         Pointer<DriveLocomotionClass> pLoco = locomotion.ToLocomotionClass<DriveLocomotionClass>();
                //         CoordStruct headTo = pLoco.Ref.HeadToCoord;
                //         BulletEffectHelper.RedCrosshair(headTo, 128);
                //         BulletEffectHelper.RedLine(pTechno.Ref.Base.Base.GetCoords(), headTo);
                //         CoordStruct destTo = pLoco.Ref.Destination;
                //         if (pTechno.Ref.Base.IsSelected || CaptureByBlackHole)
                //         {
                //             Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} speed={pLoco.Ref.Speed} mov={pLoco.Ref.MovementSpeed} IsDriving={pLoco.Ref.IsDriving}, IsRotating={pLoco.Ref.IsRotating}, IsLocked={pLoco.Ref.IsLocked}");
                //         }
                //     }
                // }
                // 被黑洞吸取中
                if (CaptureByBlackHole)
                {
                    if (pBlackHole.IsNull
                        || !pBlackHole.TryGetBlackHoleState(out BlackHoleState blackHoleState)
                        || !blackHoleState.IsActive()
                        || OutOfBlackHole(blackHoleState)
                        || !blackHoleState.IsOnMark(pTechno)
                    )
                    {
                        CancelBlackHole();
                    }
                    else
                    {
                        Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
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
                                        if (pBlackHole.CastToBullet(out Pointer<BulletClass> pBullet))
                                        {
                                            pAttacker = pBullet.Ref.Owner.Convert<ObjectClass>();
                                            pAttackingHouse = pBullet.GetSourceHouse();
                                        }
                                        else
                                        {
                                            pAttacker = pBlackHole;
                                            pAttackingHouse = pBlackHole.Convert<TechnoClass>().Ref.Owner;
                                        }
                                        // Logger.Log($"{Game.CurrentFrame} 黑洞对 [{section}]{pTechno} 造成伤害 Damage = {blackHoleData.Damage}, ROF = {blackHoleData.DamageDelay}, WH = {pWH.Ref.Base.ID}");
                                        pTechno.Ref.Base.TakeDamage(blackHoleData.Damage, pWH, pAttacker, pAttackingHouse, pTechno.Ref.Type.Ref.Crewed);
                                    }
                                }
                            }
                            // 目标设置
                            if (blackHoleData.ClearTarget)
                            {
                                pTechno.ClearAllTarget();
                            }
                            if (blackHoleData.ChangeTarget)
                            {
                                pTechno.Ref.SetTarget(pBlackHole.Convert<AbstractClass>());
                            }
                            // 失控设置
                            if (!isBuilding && blackHoleData.OutOfControl)
                            {
                                lostControl = true;
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 失去控制");
                                pTechno.ClearAllTarget();
                                pTechno.Ref.Base.Deselect();
                                pMission.Ref.ForceMission(Mission.None);
                                pMission.Ref.QueueMission(Mission.Sleep, false);
                                // pTechno.Convert<FootClass>().Ref.IsAttackedByLocomotor = true;
                            }
                        }
                        // 移动位置
                        if (!isBuilding)
                        {
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响，开始调整位置");
                            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                            // 从占据的格子中移除自己
                            pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                            Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                            // 停止移动
                            pFoot.ForceStopMoving();
                            // 计算下一个坐标点
                            // 以偏移量为FLH获取目标点
                            CoordStruct targetPos = pBlackHole.GetFLHAbsoluteCoords(blackHoleData.Offset, blackHoleData.IsOnTurret);
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
                            PassError passError = PhysicsHelper.CanMoveTo(sourcePos, nextPos, blackHoleData.AllowPassBuilding, out CoordStruct nextCellPos, out bool onBridge);
                            switch (passError)
                            {
                                case PassError.HITWALL:
                                case PassError.HITBUILDING:
                                case PassError.UPBRIDEG:
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
                                    break;
                                case PassError.UNDERGROUND:
                                case PassError.DOWNBRIDGE:
                                    // 卡在地表
                                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 砸在桥上");
                                    nextPos.Z = nextCellPos.Z;
                                    break;
                            }
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 获得新位置坐标 {nextPos} 原始位置 {sourcePos} {(!canMove ? "受到阻挡不能前进，返回" : "")}");
                            // 被黑洞吸走
                            pTechno.Ref.Base.Mark(MarkType.UP);
                            // 是否在桥上
                            pTechno.Ref.Base.OnBridge = onBridge;
                            pTechno.Ref.Base.SetLocation(nextPos);
                            // CoordStruct dest = loco.Destination();
                            // BulletEffectHelper.GreenCrosshair(nextPos, 128);
                            // BulletEffectHelper.GreenLine(sourcePos, nextPos);
                            pTechno.Ref.Base.Mark(MarkType.DOWN);
                            // 移除黑幕
                            MapClass.Instance.RevealArea2(nextPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 0);
                            MapClass.Instance.RevealArea2(nextPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 1);
                            // 设置动作
                            if (blackHoleData.AllowCrawl && isInfantry)
                            {
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置步兵匍匐动作");
                                pTechno.Convert<FootClass>().Ref.Inf_PlayAnim(SequenceAnimType.CRAWL);
                            }
                            // 设置翻滚
                            if (blackHoleData.AllowRotateUnit)
                            {
                                // if (pTechno.Ref.IsVoxel() && canMove)
                                // {
                                //     pTechno.Ref.RockingForwardsPerFrame = 0.2f;
                                //     pTechno.Ref.RockingSidewaysPerFrame = 0.2f;
                                // }
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置VXL翻滚动作");
                                // 设置朝向
                                if (lastMission == Mission.Move || lastMission == Mission.AttackMove || pTechno.Ref.Type.Ref.ConsideredAircraft || !pTechno.InAir())
                                {
                                    CoordStruct p1 = targetPos;
                                    CoordStruct p2 = sourcePos;
                                    p1.Z = 0;
                                    p2.Z = 0;
                                    if (p1.DistanceFrom(p2) >= speed)
                                    {
                                        DirStruct facingDir = FLHHelper.Point2Dir(targetPos, sourcePos);
                                        pTechno.Ref.Facing.turn(facingDir);
                                        if (isJumpjet)
                                        {
                                            // JJ朝向是单独的Facing
                                            Pointer<JumpjetLocomotionClass> pLoco = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<JumpjetLocomotionClass>();
                                            pLoco.Ref.LocomotionFacing.turn(facingDir);
                                        }
                                        else if (isAircraft)
                                        {
                                            // 飞机使用的炮塔的Facing
                                            pTechno.Ref.TurretFacing.turn(facingDir);
                                        }
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 扭头，屁股朝前");
                                    }
                                }
                            }
                        }

                    }
                }
            }

        }

        public unsafe void OnReceiveDamage2_BlackHole(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pTechno.IsDeadOrInvisible() && damageState != DamageState.NowDead
                && pWH.GetData().Capturer
                && pAttacker.TryGetBlackHoleState(out BlackHoleState state) && state.IsActive()
                && state.Data.CaptureFromWarhead && state.Data.CanAffectType(pTechno) && state.IsOnMark(pTechno))
            {
                BlackHoleEntity data = state.GetData();
                if (null != data && data.Range != 0)
                {
                    CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                    CoordStruct targetPos = pAttacker.Ref.Base.GetCoords();
                    double distance = sourcePos.DistanceFrom(targetPos);
                    if (!state.IsOutOfRange(distance))
                    {
                        SetBlackHole(state.Owner, state.Data);
                    }
                }
            }
        }

        public void SetBlackHole(IExtension blackHoleExt, BlackHoleData blackHoleData)
        {
            if (!this.CaptureByBlackHole || null == this.blackHoleData || this.blackHoleData.Weight < blackHoleData.Weight || ((this.blackHoleData.Weight == blackHoleData.Weight || blackHoleData.Weight <= 0) && blackHoleData.CaptureFromSameWeight))
            {
                this.blackHoleExt = blackHoleExt;
                this.blackHoleData = blackHoleData;
                this.CaptureByBlackHole = true;
            }
        }

        public void CancelBlackHole()
        {
            // Logger.Log($"{Game.CurrentFrame} 单位 [{section}]{pTechno} 不再受 黑洞 {pBlackHole.Pointer} 的影响");
            if (CaptureByBlackHole && !isBuilding && !pTechno.IsDeadOrInvisible())
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
                int fallingDestroyHeight = 0;
                if (null != blackHoleData && blackHoleData.AllowFallingDestroy)
                {
                    fallingDestroyHeight = blackHoleData.FallingDestroyHeight;
                }
                FallingDown(fallingDestroyHeight, false);
            }
            this.CaptureByBlackHole = false;
            this.blackHoleData = null;
            this.lostControl = false;
            this.blackHoleExt = null;
        }

        /// <summary>
        /// 结束运动后摔死
        /// </summary>
        /// <param name="fallingDestroyHeight"></param>
        /// <param name="hasParachute"></param>
        public bool FallingDown(int fallingDestroyHeight, bool hasParachute)
        {
            // 检查是否在悬崖上摔死
            bool canPass = true;
            bool isWater = false;
            CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
            CoordStruct targetPos = location;
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {
                CoordStruct cellPos = pCell.Ref.GetCoordsWithBridge();
                pTechno.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();

                // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  位于桥上 {pCell.Ref.ContainsBridge()} {pTechno.Ref.Base.GetHeight()}， 桥高 {cellPos.Z}");
                if (cellPos.Z >= location.Z)
                {
                    targetPos.Z = cellPos.Z;
                    // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  位于地下 {pTechno.Ref.Base.GetHeight()}，调整回地表");
                    pTechno.Ref.Base.SetLocation(targetPos);
                }
                // 当前格子所在的位置不可通行，炸了它
                canPass = pCell.Ref.IsClearToMove(pTechno.Ref.Type.Ref.SpeedType, pTechno.Ref.Type.Ref.MovementZone, true, true);
                // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  当前格子可通行 = {canPass}");
                if (canPass && !pCell.Ref.GetBuilding().IsNull)
                {
                    canPass = false;
                }
                if (!canPass)
                {
                    isWater = pCell.Ref.TileIs(TileType.Water);
                }

            }
            int height = pTechno.Ref.Base.GetHeight();
            bool inAir = height >= Game.LevelHeight * 2;
            if (inAir && pTechno.Ref.Type.Ref.ConsideredAircraft)
            {
                // 飞行器在天上，免死
                // Logger.Log($"{Game.CurrentFrame} 飞机 恢复行动");
                pTechno.Ref.SetDestination(pCell);
                if (pTechno.Ref.Target.IsNull)
                {
                    if (isAircraft)
                    {
                        pTechno.Ref.BaseMission.QueueMission(Mission.Enter, false);
                    }
                    else
                    {
                        pTechno.Ref.BaseMission.QueueMission(Mission.Guard, false);
                    }
                }
                else
                {
                    pTechno.Ref.BaseMission.QueueMission(Mission.Attack, false);
                }
            }
            else
            {
                bool drop = false;
                bool sinking = false;
                // 检查下方是不是水
                if (isWater)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}] {pTechno} 下方是水 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                    switch (locoType)
                    {
                        case LocoType.Hover:
                        case LocoType.Ship:
                            // 船和悬浮不下沉
                            canPass = true;
                            break;
                        case LocoType.Jumpjet:
                            if (!pTechno.Ref.Type.Ref.BalloonHover)
                            {
                                sinking = true;
                            }
                            break;
                        default:
                            sinking = true;
                            break;
                    }
                }
                // 高度大于一定值时强制摔死
                if (fallingDestroyHeight > 0 && height >= fallingDestroyHeight)
                {
                    canPass = false;
                }
                if (canPass)
                {
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
                    if (height <= 0 && sinking)
                    {
                        // Logger.Log($"{Game.CurrentFrame} {(IsBuilding ? "建筑" : "单位")} [{section}] {pTechno} 下方不可通行 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        pTechno.Ref.IsSinking = true;
                    }
                    else
                    {

                        // Logger.Log($"{Game.CurrentFrame} {(IsBuilding ? "建筑" : "单位")} [{section}] {pTechno} 下方不可通行 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        pTechno.Ref.Base.DropAsBomb();
                        drop = true;
                    }
                }
                if (drop)
                {
                    if (hasParachute && inAir)
                    {
                        // ObjectClass.SpawnParachuted(Coords)需要检查单位Unlimbo成功，此处手动添加降落伞动画
                        Pointer<AnimTypeClass> pAnimType = RulesClass.Global().Parachute;
                        if (!pAnimType.IsNull)
                        {
                            CoordStruct parachutePos = targetPos;
                            parachutePos.Z += 75;
                            Pointer<AnimClass> pAnim = YRMemory.Create<AnimClass>(pAnimType, parachutePos, 0, 1, BlitterFlags.bf_400 | BlitterFlags.Centered, 0, false);
                            pTechno.Ref.Base.Parachute = pAnim;
                            pAnim.Ref.SetOwnerObject(pTechno.Convert<ObjectClass>());
                            pAnim.Ref.Owner = pTechno.Ref.Owner;
                        }
                    }
                    else if (isInfantry)
                    {
                        pTechno.Convert<FootClass>().Ref.Inf_PlayAnim(SequenceAnimType.Paradrop);
                    }
                }
            }
            return !canPass;
        }

        private bool OutOfBlackHole(BlackHoleState blackHoleState)
        {
            CoordStruct taregtPos = pBlackHole.Ref.Base.GetCoords();
            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
            // BulletEffectHelper.BlueCrosshair(taregtPos, 128);
            // BulletEffectHelper.BlueCell(sourcePos, 128);
            // BulletEffectHelper.BlueLine(sourcePos, taregtPos);
            return blackHoleState.IsOutOfRange(taregtPos.DistanceFrom(sourcePos));
        }

    }
}
