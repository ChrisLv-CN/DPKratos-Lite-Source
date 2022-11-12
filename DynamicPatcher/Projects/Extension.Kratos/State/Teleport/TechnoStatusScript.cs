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
    public enum TeleportStep
    {
        NONE = 0, READY = 1, TELEPORTED = 2, FREEZING = 3, MOVEFORWARD = 4
    }

    public partial class TechnoStatusScript
    {

        public TeleportState TeleportState = new TeleportState();

        private TeleportStep teleportStep;

        private CoordStruct warpTo; // 弹头传进来的坐标
        private TeleportData data;
        private ILocomotion loco = null; // 传送的loco
        private TimerStruct teleportTimer; // 传送冰冻时间

        private SwizzleablePointer<AbstractClass> pDest = new SwizzleablePointer<AbstractClass>(IntPtr.Zero);
        private SwizzleablePointer<AbstractClass> pFocus = new SwizzleablePointer<AbstractClass>(IntPtr.Zero);

        public void InitState_Teleport()
        {
            // 初始化状态机
            if (!isBuilding)
            {
                teleportStep = TeleportStep.READY;
                // 初始化状态机
                TeleportData data = Ini.GetConfig<TeleportData>(Ini.RulesDependency, section).Data;
                if (data.Enable)
                {
                    TeleportState.Enable(data);
                }
            }
        }

        public void OnUpdate_Teleport()
        {
            if (!isBuilding)
            {
                switch (teleportStep)
                {
                    case TeleportStep.READY:
                        // 可以准备跳了
                        if (TeleportState.IsReady())
                        {
                            data = TeleportState.Data;
                            CoordStruct targetPos = default;
                            switch (data.Mode)
                            {
                                case TeleportMode.MOVE:
                                    targetPos = GetAndMarkDestination();
                                    break;
                                case TeleportMode.WARHEAD:
                                    targetPos = warpTo;
                                    warpTo = default;
                                    break;
                                case TeleportMode.BOTH:
                                    if (default != warpTo)
                                    {
                                        targetPos = warpTo;
                                        warpTo = default;
                                    }
                                    else
                                    {
                                        targetPos = GetAndMarkDestination();
                                    }
                                    break;
                                default:
                                    return;
                            }
                            if (default != targetPos)
                            {
                                // 传送距离检查
                                CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                                double distance = targetPos.DistanceFrom(sourcePos);
                                if (distance > data.RangeMin * 256 && (data.RangeMax < 0 ? true : distance < data.RangeMax * 256))
                                {
                                    // 在可以传送的范围内
                                    if (data.Distance > 0 && distance > data.Distance)
                                    {
                                        // 有限距离的传送，重新计算目标位置
                                        targetPos = FLHHelper.GetForwardCoords(sourcePos, targetPos, data.Distance, distance);
                                    }
                                }
                                else
                                {
                                    // 不可以传送
                                    targetPos = default;
                                }
                            }
                            bool teleporting = default != targetPos;
                            if (teleporting)
                            {
                                if (!data.Super)
                                {
                                    if (MapClass.Instance.TryGetCellAt(targetPos, out Pointer<CellClass> pCell) && !pCell.Ref.IsClearToMove(pTechno.Ref.Type.Ref.SpeedType, pTechno.Ref.Type.Ref.MovementZone, true, true))
                                    {
                                        // 不能通过，需要找一个新的落脚点
                                        teleporting = false;
                                    }
                                }
                                // 可以跳
                                if (teleporting)
                                {
                                    if (data.ClearTarget)
                                    {
                                        // 清除目标
                                        ClearTarget();
                                    }
                                    // Warp
                                    if (data.Super)
                                    {
                                        // 使用超武跳
                                        pTechno.Convert<FootClass>().Ref.ChronoWarpTo(targetPos);
                                    }
                                    else
                                    {
                                        // 普通跳
                                        // 停止行动
                                        StopMoving();
                                        Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                                        // 切换Loco
                                        LocomotionClass.ChangeLocomotorTo(pFoot, LocomotionClass.Teleport);
                                        loco = pTechno.Convert<FootClass>().Ref.Locomotor;
                                        loco.Move_To(targetPos);
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 变成CLEG，跳到目的地，{pTechno.Ref.ChronoLockRemaining}，冰冻时间 {teleportTimer.GetTimeLeft()}");
                                    }
                                    TeleportState.Reload();
                                    teleportStep = TeleportStep.TELEPORTED;
                                }
                            }
                        }
                        break;
                    case TeleportStep.TELEPORTED:
                        if (data.Super)
                        {
                            // 超武跳，不用冷冻计时器
                            if (!pTechno.Ref.WarpingOut)
                            {
                                if (data.MoveForward)
                                {
                                    teleportStep = TeleportStep.MOVEFORWARD;
                                }
                                else
                                {
                                    teleportStep = TeleportStep.READY;
                                }
                            }
                        }
                        else
                        {
                            // 当前帧切换loco后会切回来，而且下一帧才可以获得计时器
                            teleportTimer = loco.ToLocomotionClass<TeleportLocomotionClass>().Ref.Timer;
                            if (teleportTimer.Expired())
                            {
                                // 解冻，进入下一个阶段
                                pTechno.Ref.WarpingOut = false;
                                if (data.MoveForward)
                                {
                                    teleportStep = TeleportStep.MOVEFORWARD;
                                }
                                else
                                {
                                    teleportStep = TeleportStep.READY;
                                }
                            }
                            else
                            {
                                teleportStep = TeleportStep.FREEZING;
                            }
                        }
                        break;
                    case TeleportStep.FREEZING:
                        if (teleportTimer.Expired() || !pTechno.Ref.WarpingOut)
                        {
                            // 解冻，进入下一个阶段
                            pTechno.Ref.WarpingOut = false;
                            if (data.MoveForward)
                            {
                                teleportStep = TeleportStep.MOVEFORWARD;
                            }
                            else
                            {
                                teleportStep = TeleportStep.READY;
                            }
                        }
                        break;
                    case TeleportStep.MOVEFORWARD:
                        teleportStep = TeleportStep.READY;
                        if (pTechno.Ref.Target.IsNull)
                        {
                            // 把移动目的地，设回去
                            if (!pFocus.IsNull)
                            {
                                pTechno.Ref.SetFocus(pFocus);
                            }
                            if (!pDest.IsNull)
                            {
                                pTechno.Ref.SetDestination(pDest, true);
                                pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Move, true);
                            }
                        }
                        else
                        {
                            pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Attack, true);
                        }
                        pFocus.Pointer = IntPtr.Zero;
                        pDest.Pointer = IntPtr.Zero;
                        break;
                }
            }
        }

        private CoordStruct GetAndMarkDestination()
        {
            ILocomotion loco = pTechno.Convert<FootClass>().Ref.Locomotor;
            CoordStruct targetPos = loco.Destination();
            // 记录下目的地
            pDest = pTechno.Convert<FootClass>().Ref.Destination;
            pFocus = pTechno.Ref.Focus;
            return targetPos;
        }

        public void Teleport(Pointer<CoordStruct> pLocation, Pointer<WarheadTypeClass> pWH)
        {
            if (teleportStep == TeleportStep.READY)
            {
                CoordStruct location = pLocation.Ref;
                if (default != location)
                {
                    WarheadTypeData data = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                    if (data.Teleporter)
                    {
                        this.warpTo = location;
                    }
                }
            }
        }

    }
}
