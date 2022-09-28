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
                    CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                    // 从占据的格子中移除自己
                    pTechno.Ref.Base.UnmarkAllOccupationBits(sourcePos);
                    // 停止移动
                    Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                    ILocomotion loco = pFoot.Ref.Locomotor;
                    loco.Stop_Moving();
                    pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Stop, true);
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
                                    break;
                            }
                        }
                        // 检查建筑

                    }
                    if (canMove)
                    {
                        // 被黑洞吸走
                        pTechno.Ref.Base.SetLocation(nextPos);
                    }
                    else
                    {
                        // 反弹回移动前的格子
                        if (MapClass.Instance.TryGetCellAt(sourcePos, out Pointer<CellClass> pSourceCell))
                        {
                            pTechno.Ref.Base.SetLocation(pSourceCell.Ref.GetCoordsWithBridge());
                        }
                    }
                    // 设置朝向
                    if (lastMission == Mission.Move || lastMission == Mission.AttackMove || pTechno.InAir())
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
                    }
                }
            }
            // 黑洞吸人
            if (BlackHoleState.IsReady())
            {
                BlackHoleState.Capture(pTechno.Convert<ObjectClass>(), pTechno.Ref.Owner);
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
                // 散开
                pTechno.Ref.Base.Scatter(default, true, false);
                // 飞机恢复升空
                if (pTechno.Ref.Base.Base.IsOnFloor() && pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                {
                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                    {
                        pTechno.Ref.SetDestination(pCell, true);
                        pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Move, false);
                    }
                }
            }
            CaptureByBlackHole = false;
        }

    }
}
