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

        private bool isHumanCannon; // 人间大炮
        private TimerStruct flyTimer; // 飞行时间

        private int gravity;
        private BulletVelocity velocity; // 初始向量
        private CoordStruct jumpTo; // 跳到的目的地

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
                    if (CaptureByBlackHole || (isHumanCannon && flyTimer.Expired()))
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

                    // 初速度削减重力，下一个坐标位置
                    velocity.Z -= gravity;
                    CoordStruct nextPos = sourcePos + velocity.ToCoordStruct();
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

        public void HumanCannon(CoordStruct sourcePos, CoordStruct targetPos, int height, bool isLobber = false)
        {
            if (pTechno.Ref.Passengers.NumPassengers > 0)
            {
                // 人间大炮一级准备
                Pointer<FootClass> pPassenger = pTechno.Ref.Passengers.RemoveFirstPassenger();
                DirStruct facing = pTechno.Ref.GetRealFacing().current();
                ++Game.IKnowWhatImDoing;
                pPassenger.Ref.Base.Base.Put(sourcePos, facing.ToDirType());
                --Game.IKnowWhatImDoing;
                // 人间大炮二级准备
                if (pPassenger.Convert<TechnoClass>().TryGetStatus(out TechnoStatusScript status))
                {
                    // 人间大炮发射
                    targetPos += new CoordStruct(0, 0, height);
                    status.Jump(targetPos, isLobber, true);
                }
            }
        }

        public bool PumpAction(CoordStruct targetPos, bool isLobber)
        {
            if (!isBuilding && !pTechno.Ref.Base.IsFallingDown && !pumpLock && !AmIStand())
            {
                // 跳
                return Jump(targetPos, isLobber);
            }
            return false;
        }

        private bool Jump(CoordStruct targetPos, bool isLobber, bool isHumanCannon = false)
        {
            CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
            int gravity = RulesClass.Global().Gravity;
            // 计算初速度
            BulletVelocity velocity = WeaponHelper.GetBulletArcingVelocity(sourcePos, targetPos, 0, gravity, isLobber, gravity, out double straightDistance, out double realSpeed);
            // 跳
            if (straightDistance > 256 && JumpAction(targetPos, velocity, gravity, straightDistance))
            {
                if (this.isHumanCannon = isHumanCannon)
                {
                    // 计算飞行时间
                    int frame = (int)(straightDistance / realSpeed);
                    this.flyTimer.Start(frame);
                }
                // 从占据的格子中移除自己
                pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                // 停止移动
                pFoot.ForceStopMoving();
                // pTechno.Ref.BaseMission.ForceMission(Mission.None);
                return true;
            }
            return false;
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
            if (JumpAction(targetPos, velocity, data.Gravity, straightDistance))
            {
                // 清空所有目标和任务
                Pointer<MissionClass> pMission = pTechno.Convert<MissionClass>();
                pTechno.ClearAllTarget();
                pMission.Ref.ForceMission(Mission.None);
            }

        }

        private bool JumpAction(CoordStruct targetPos, BulletVelocity velocity, int gravity, double straightDistance)
        {
            if (default != velocity)
            {
                this.jumpTo = targetPos;
                this.velocity = velocity;
                this.gravity = gravity;
                this.Jumping = true;
                pTechno.Ref.Base.IsFallingDown = false; // 强设为false
                return true;
            }
            return false;
        }

        private void CancelPump()
        {
            PumpState.Disable();
            this.Jumping = false;
            this.velocity = default;
            this.jumpTo = default;
            this.gravity = 0;
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 取消移动");
            if (!CaptureByBlackHole && !pTechno.IsDeadOrInvisible())
            {
                FallingDown(0, isHumanCannon);
            }
            this.isHumanCannon = false;
            this.flyTimer.Stop();
        }


    }
}
