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

    public partial class TechnoStatusScript
    {

        public State<PumpData> PumpState = new State<PumpData>();
        public bool Jumping;

        private PumpData pumpData;
        private BulletVelocity velocity; // 初始向量
        private CoordStruct jumpTo; // 跳到的目的地
        private TimerStruct flyTimer; // 滞空时间

        private bool pumpLock; // 不再接受新的状态

        public void InitState_Pump()
        {
            // 初始化状态机
            if (!isBuilding)
            {
                // 初始化状态机
                PumpData data = Ini.GetConfig<PumpData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    PumpState.Enable(data);
                }
            }
        }

        public void OnUpdate_Pump()
        {
            if (!isBuilding && !pTechno.Ref.Base.IsFallingDown)
            {
                if (PumpState.IsActive() && PumpState.IsReset())
                {
                    ActivePump(PumpState.Data, PumpState.AE.pSourceHouse, PumpState.AE.WarheadLocation);
                }
                if (Jumping)
                {
                    if (CaptureByBlackHole)
                    {
                        CancelPump();
                        return;
                    }
                    // 咱蹦高了
                    CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                    // 从占据的格子中移除自己
                    pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                    Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                    // 停止移动
                    pFoot.ForceStopMoving();
                    // ILocomotion loco = pFoot.Ref.Locomotor;
                    // LocomotionClass.ChangeLocomotorTo(pFoot, LocomotionClass.Jumpjet);
                    // 计算下一个坐标点
                    CoordStruct nextPos = GetNextPos(sourcePos);
                    if (default == nextPos)
                    {
                        // 没算出速度
                        CancelPump();
                        return;
                    }
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 获得下一个坐标点 {nextPos}, 当前坐标点 {sourcePos}");
                    PassError passError = PhysicsHelper.CanMoveTo(sourcePos, nextPos, false, out CoordStruct nextCellPos, out bool onBridge);
                    switch (passError)
                    {
                        case PassError.HITWALL:
                        case PassError.HITBUILDING:
                        case PassError.UPBRIDEG:
                            // 反弹
                            velocity.X *= -1;
                            velocity.Y *= -1;
                            nextPos = sourcePos + velocity.ToCoordStruct();
                            passError = PhysicsHelper.CanMoveTo(sourcePos, nextPos, false, out nextCellPos, out onBridge);
                            break;
                        case PassError.UNDERGROUND:
                        case PassError.DOWNBRIDGE:
                            // 卡在地表
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 砸在桥上");
                            nextPos = nextCellPos;
                            break;
                    }
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
                    switch (passError)
                    {
                        case PassError.UNDERGROUND:
                        case PassError.HITWALL:
                        case PassError.HITBUILDING:
                        case PassError.DOWNBRIDGE:
                            // 反弹后仍然触底或者撞悬崖
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 下一个坐标点 {nextPos} 无法抵达，终止，当前坐标点 {sourcePos}");
                            // 掉落地面
                            CancelPump();
                            break;
                    }
                }
            }
        }

        private void ActivePump(PumpData data, Pointer<HouseClass> pAttackingHouse, CoordStruct powerPos = default)
        {
            if (pumpLock
                || !data.Enable
                || AmIStand() // 替身绝对不许跳
                || (!data.AffectInAir && pTechno.InAir())
                || !data.CanAffectType(pTechno)
                || !data.CanAffectHouse(pAttackingHouse, pTechno.Ref.Owner)
                || !data.IsOnMark(pTechno)
            )
            {
                // 蚌埠高
                return;
            }
            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
            // 计算初速度
            double straightDistance = 0;
            double realSpeed = 0;
            BulletVelocity velocity = default;
            CoordStruct targetPos = default;
            if (default == powerPos || sourcePos == powerPos)
            {
                targetPos = sourcePos;
                // 原地起跳，但不精确，随机落点，作为目的地，绘制抛物线
                if (data.Inaccurate)
                {
                    targetPos = sourcePos + WeaponHelper.GetInaccurateOffset(data.ScatterMin, data.ScatterMax);
                }
                if (sourcePos == targetPos)
                {
                    // 随机过还是原地起跳，啧啧
                    velocity = new BulletVelocity(0, 0, data.Gravity);
                    // WWSB，计算滞空时间是用距离，如果直上直下，距离是跳跃高度
                    straightDistance = data.Range * 256;
                    realSpeed = Math.Sqrt(straightDistance * data.Gravity * 1.2);
                    if (data.Lobber)
                    {
                        realSpeed = (int)(realSpeed * 0.5);
                    }
                }
                else
                {
                    velocity = WeaponHelper.GetBulletArcingVelocity(sourcePos, ref targetPos, 0, data.Gravity, data.Lobber, false, 0, 0, data.Gravity, out straightDistance, out realSpeed, out Pointer<CellClass> pTargetCell);
                }
                // Logger.Log($"{Game.CurrentFrame} 原地起跳 距离 {straightDistance} 速度 {realSpeed}");
            }
            else
            {
                // 目的地是从发力点到当前位置的沿线距离后的一点
                int range = data.Range * 256; // 影响的范围
                int dist = (int)sourcePos.DistanceFrom(powerPos); // 单位和爆心的距离
                int forward = data.PowerBySelf ? range : range - dist;
                // Logger.Log($"{Game.CurrentFrame} 单位离爆心的距离 {dist} 范围 {range}");
                if (forward > 0)
                {
                    // 单位在爆炸范围内
                    // Logger.Log($"{Game.CurrentFrame} 往前跳 {dist + forward}");
                    targetPos = FLHHelper.GetForwardCoords(powerPos, sourcePos, dist + forward);
                    velocity = WeaponHelper.GetBulletArcingVelocity(sourcePos, ref targetPos, 0, data.Gravity, data.Lobber, data.Inaccurate, data.ScatterMin, data.ScatterMax, data.Gravity, out straightDistance, out realSpeed, out Pointer<CellClass> pTargetCell);
                }
            }
            // Logger.Log($"{Game.CurrentFrame} 得到弹道初速度 {velocity}");
            if (default != velocity)
            {
                this.velocity = velocity;
                this.jumpTo = targetPos;
                this.pumpData = data;
                this.Jumping = true;
                pTechno.Ref.Base.IsFallingDown = false; // 强设为false
                // 飞行时间
                realSpeed = realSpeed == 0 ? Math.Sqrt(straightDistance * data.Gravity * 1.2) : realSpeed;
                int frames = (int)(straightDistance / realSpeed);
                // Logger.Log($"{Game.CurrentFrame} 飞行速度 {realSpeed}, 直线距离{straightDistance}, 飞 {frames} 帧");
                this.flyTimer.Start(frames / 2);
                // 清空所有目标和任务
                Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
                pTechno.ClearAllTarget();
                pMission.Ref.ForceMission(Mission.None);
            }
        }

        private void CancelPump()
        {
            PumpState.Disable();
            this.Jumping = false;
            this.velocity = default;
            this.jumpTo = default;
            this.pumpData = null;
            this.flyTimer.Stop();
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 取消移动");
            if (!CaptureByBlackHole && !pTechno.IsDeadOrInvisible())
            {
                // 摔死
                // 检查是否在悬崖上摔死
                bool canPass = true;
                bool isWater = false;
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    CoordStruct cellPos = pCell.Ref.GetCoordsWithBridge();
                    pTechno.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();
                    // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  位于桥上 {pCell.Ref.ContainsBridge()} {pTechno.Ref.Base.GetHeight()}， 桥高 {cellPos.Z}");
                    if (cellPos.Z >= location.Z)
                    {
                        CoordStruct targetPos = location;
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
                if (canPass)
                {
                    // 掉地上
                    if (pTechno.Ref.Base.GetHeight() > 0)
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 掉落");
                        pTechno.Ref.Base.IsFallingDown = true;
                    }
                }
                else
                {
                    // 底下是水吗
                    if (isWater)
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}] {pTechno} 下方是水 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        switch (locoType)
                        {
                            case LocoType.Hover:
                            case LocoType.Ship:
                                // 船和悬浮不下沉
                                break;
                            case LocoType.Jumpjet:
                                if (!pTechno.Ref.Type.Ref.BalloonHover)
                                {
                                    pTechno.Ref.IsSinking = true;
                                    // 摔死
                                    pumpLock = true;
                                }
                                break;
                            default:
                                pTechno.Ref.IsSinking = true;
                                // 摔死
                                pumpLock = true;
                                break;
                        }
                    }
                    else
                    {
                        // 摔死
                        pumpLock = true;
                        // Logger.Log($"{Game.CurrentFrame} [{section}] {pTechno} 下方不可通行 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        pTechno.Ref.Base.DropAsBomb();
                    }
                }
            }
        }

        public CoordStruct GetNextPos(CoordStruct sourcePos)
        {
            // 初速度削减重力，下一个坐标位置
            if (flyTimer.Expired())
            {
                velocity.Z -= pumpData.Gravity;
            }
            else
            {
                velocity.Z += pumpData.Gravity;
            }
            // Logger.Log($"移动速度{velocity}, 移动距离 {velocity.ToCoordStruct()}");
            return sourcePos + velocity.ToCoordStruct();
        }


    }
}
