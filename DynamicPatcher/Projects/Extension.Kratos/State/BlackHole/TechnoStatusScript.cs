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

        public BlackHoleState BlackHoleState = new BlackHoleState();

        public bool CaptureByBlackHole;
        public SwizzleablePointer<ObjectClass> pBlackHole = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);

        public void OnPut_BlackHole()
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
            // 黑洞吸人
            if (BlackHoleState.IsReady())
            {
                BlackHoleState.Capture(pTechno.Convert<ObjectClass>(), pTechno.Ref.Owner);
            }
            // 被黑洞吸取中
            if (CaptureByBlackHole)
            {
                if (IsBuilding || pBlackHole.IsNull
                    || (pBlackHole.Pointer.BlackHoleStateDone()))
                {
                    CancelBlackHole();
                }
                else
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响，开始调整位置");
                    CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                    // 从占据的格子中移除自己
                    pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);

                    // 停止移动
                    Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                    ILocomotion loco = pFoot.Ref.Locomotor;
                    loco.Stop_Moving();
                    pTechno.Convert<MissionClass>().Ref.ForceMission(Mission.None);
                    pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Stop, false);
                    loco.Mark_All_Occupation_Bits(0);
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 停止行动");
                    // 计算下一个坐标点
                    CoordStruct targetPos = pBlackHole.Ref.Base.GetCoords();
                    // 获取偏移量
                    if (pBlackHole.Pointer.TryGetBlackHoleState(out BlackHoleState blackHoleState) && blackHoleState.IsActive())
                    {
                        targetPos += blackHoleState.Data.Offset;
                    }
                    int speed = pTechno.Ref.GetDefaultSpeed();
                    CoordStruct nextPosFLH = new CoordStruct(speed, 0, 0);
                    DirStruct nextPosDir = ExHelper.Point2Dir(sourcePos, targetPos);
                    CoordStruct nextPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, nextPosFLH, nextPosDir);
                    // 计算Z值
                    int deltaZ = sourcePos.Z - targetPos.Z;
                    if (deltaZ < 0)
                    {
                        // 目标点在上方
                        int offset = -deltaZ > 20 ? 20 : -deltaZ;
                        nextPos.Z += offset;
                    }
                    else if (deltaZ > 0)
                    {
                        // 目标点在下方
                        int offset = deltaZ > 20 ? 20 : deltaZ;
                        nextPos.Z -= offset;
                    }
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
                                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到悬崖 {(canMove ? "可通过" : "不可通过")}");
                                    break;
                            }
                        }
                        // 检查建筑
                        // 会飞的单位不检查建筑
                        if (!pTechno.Ref.Type.Ref.ConsideredAircraft)
                        {
                            Pointer<BuildingClass> pBuilding = pTargetCell.Ref.GetBuilding();
                            if (!pBuilding.IsNull)
                            {
                                canMove = !pBuilding.CanHit(nextPos.Z);
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 行进路线遇到建筑 [{pBuilding.Ref.Type.Ref.Base.Base.Base.ID}] {pBuilding} {(canMove ? "可通过" : "不可通过")}");
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
                    pTechno.Ref.Base.SetLocation(nextPos);
                    // 设置动作
                    if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Infantry)
                    {
                        pFoot.Ref.Inf_PlayAnim(SequenceAnimType.CRAWL);
                    }
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置步兵匍匐动作");
                    if (pTechno.Ref.IsVoxel() && canMove)
                    {
                        // pTechno.Ref.RockingForwardsPerFrame = 0.2f;
                        // pTechno.Ref.RockingSidewaysPerFrame = 0.2f;
                    }
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 设置VXL翻滚动作");
                    // 设置朝向
                    if (lastMission == Mission.Move || lastMission == Mission.AttackMove || pTechno.Ref.Base.GetHeight() > 0)
                    {
                        DirStruct facingDir = ExHelper.Point2Dir(targetPos, sourcePos);
                        pTechno.Ref.Facing.turn(facingDir);
                        pTechno.Ref.GetRealFacing().turn(facingDir);
                        if (loco.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Jumpjet)
                        {
                            // JJ朝向是单独的Facing
                            Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                            pLoco.Ref.LocomotionFacing.turn(facingDir);
                        }
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 扭头，屁股朝前");
                    }
                }
            }

        }

        public void SetBlackHole(Pointer<ObjectClass> pBlackHole)
        {
            this.pBlackHole.Pointer = pBlackHole;
            this.CaptureByBlackHole = true;
        }

        public void CancelBlackHole()
        {
            if (CaptureByBlackHole && !pTechno.IsDeadOrInvisible())
            {
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
                if (!canPass && (!pTechno.Ref.Type.Ref.ConsideredAircraft || !pTechno.InAir()))
                {
                    // 摔死
                    pTechno.Ref.RockingForwardsPerFrame = 0.2f;
                    pTechno.Ref.RockingSidewaysPerFrame = 0.2f;
                    pTechno.Ref.Base.DropAsBomb();
                }
                else
                {
                    if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                    {
                        // 飞机起飞
                        pTechno.Ref.SetDestination(pCell);
                        pTechno.Convert<MissionClass>().Ref.ForceMission(Mission.None);
                        pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Move, false);
                    }
                    else if (pTechno.Ref.Base.GetHeight() > 0)
                    {
                        pTechno.Ref.Base.UnmarkAllOccupationBits(targetPos);
                        pTechno.Ref.Base.IsFallingDown = true;
                    }
                    else
                    {
                        pTechno.Ref.Base.Scatter(targetPos, true, true);
                    }
                }
            }
            CaptureByBlackHole = false;
        }

    }
}
