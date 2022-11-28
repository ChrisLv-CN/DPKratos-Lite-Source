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
    public enum PassError
    {
        PASS = 0, HITWALL = 1, UNDERGROUND = 2
    }

    public partial class TechnoStatusScript
    {

        public PumpState PumpState = new PumpState();
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
            if (!isBuilding)
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
                    // 停止移动
                    StopMoving();
                    // 计算下一个坐标点
                    CoordStruct nextPos = GetNextPos(sourcePos);
                    if (default == nextPos)
                    {
                        // 没算出速度
                        CancelPump();
                        return;
                    }
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 获得下一个坐标点 {nextPos}, 当前坐标点 {sourcePos}");
                    PassError canMove = CanMoveTo(sourcePos, nextPos, false);
                    if (canMove == PassError.HITWALL)
                    {
                        // 反弹
                        velocity.X *= -1;
                        velocity.Y *= -1;
                        nextPos = sourcePos + velocity.ToCoordStruct();
                        canMove = CanMoveTo(sourcePos, nextPos, false);
                    }
                    // 被黑洞吸走
                    pTechno.Ref.Base.Mark(MarkType.UP);
                    pTechno.Ref.Base.SetLocation(nextPos);
                    // CoordStruct dest = loco.Destination();
                    // BulletEffectHelper.GreenCrosshair(nextPos, 128);
                    // BulletEffectHelper.GreenLine(sourcePos, nextPos);
                    pTechno.Ref.Base.Mark(MarkType.DOWN);
                    // 移除黑幕
                    MapClass.Instance.RevealArea2(nextPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 0);
                    MapClass.Instance.RevealArea2(nextPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 1);
                    switch (canMove)
                    {
                        case PassError.UNDERGROUND:
                        case PassError.HITWALL:
                            // 反弹后仍然触底或者撞悬崖
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 下一个坐标点 {nextPos} 无法抵达，终止，当前坐标点 {sourcePos}");
                            // 掉落地面
                            CancelPump();
                            break;
                    }
                }
            }
        }

        private PassError CanMoveTo(CoordStruct sourcePos, CoordStruct nextPos, bool passBuilding)
        {
            PassError canPass = PassError.PASS;
            int deltaZ = sourcePos.Z - nextPos.Z;
            // 检查地面
            if (MapClass.Instance.TryGetCellAt(nextPos, out Pointer<CellClass> pTargetCell))
            {
                CoordStruct cellPos = pTargetCell.Ref.GetCoordsWithBridge();
                if (cellPos.Z >= nextPos.Z)
                {
                    // 沉入地面
                    nextPos.Z = cellPos.Z;
                    canPass = PassError.UNDERGROUND;
                    // 检查悬崖
                    switch (pTargetCell.Ref.GetTileType())
                    {
                        case TileType.Cliff:
                        case TileType.DestroyableCliff:
                            // 悬崖上可以往悬崖下移动
                            if (deltaZ <= 0)
                            {
                                canPass = PassError.HITWALL;
                            }
                            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到悬崖 {(canMove ? "可通过" : "不可通过")} nextPos = {nextPos}");
                            break;
                    }
                }
                // 检查建筑
                if (!passBuilding)
                {
                    Pointer<BuildingClass> pBuilding = pTargetCell.Ref.GetBuilding();
                    if (!pBuilding.IsNull)
                    {
                        if (pBuilding.CanHit(nextPos.Z))
                        {
                            canPass = PassError.HITWALL;
                        }
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到建筑 [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] {pBuilding} {(canMove ? "可通过" : "不可通过")} nextPos {nextPos}");
                    }
                }
            }
            return canPass;
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
                    targetPos = sourcePos + BulletTypeHelper.GetInaccurateOffset(data.ScatterMin, data.ScatterMax);
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
                    velocity = BulletTypeHelper.GetBulletArcingVelocity(sourcePos, targetPos, 0, data.Gravity, data.Lobber, false, 0, 0, out straightDistance, out realSpeed);
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
                    velocity = BulletTypeHelper.GetBulletArcingVelocity(sourcePos, targetPos, 0, data.Gravity, data.Lobber, data.Inaccurate, data.ScatterMin, data.ScatterMax, out straightDistance, out realSpeed);
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
            if (!CaptureByBlackHole && !pTechno.IsDeadOrInvisible())
            {
                // 摔死
                // 检查是否在悬崖上摔死
                bool canPass = true;
                bool isWater = false;
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                CoordStruct targetPos = location;
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    targetPos = pCell.Ref.GetCoordsWithBridge();
                    if (pTechno.Ref.Base.GetHeight() < 0)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 单位  [{section}] {pTechno}  位于地下 {pTechno.Ref.Base.GetHeight()}，调整回地表");
                        pTechno.Ref.Base.SetLocation(targetPos);
                    }
                    // 当前格子所在的位置不可通行，炸了它
                    canPass = pCell.Ref.IsClearToMove(pTechno.Ref.Type.Ref.SpeedType, pTechno.Ref.Type.Ref.MovementZone, true, true);
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
                        pTechno.Ref.Base.IsFallingDown = true;
                    }
                }
                else
                {
                    // 摔死
                    pumpLock = true;
                    // 底下是水吗
                    if (isWater && pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Unit)
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}] {pTechno} 下方是水 高度 {pTechno.Ref.Base.GetHeight()}，弄死");
                        pTechno.Ref.IsSinking = true;
                    }
                    else
                    {
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
