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
                                    loco = null;
                                    // Warp
                                    if (pTechno.InAir())
                                    {
                                        // 空中跳，自定义跳
                                        CoordStruct sourcePos = pTechno.Ref.Base.Base.GetCoords();
                                        int height = pTechno.Ref.Base.GetHeight();
                                        targetPos.Z += height;
                                        // 移动位置
                                        pTechno.Ref.Base.Mark(MarkType.UP);
                                        pTechno.Ref.Base.SetLocation(targetPos);
                                        pTechno.Ref.Base.Mark(MarkType.DOWN);
                                        // 移除黑幕
                                        MapClass.Instance.RevealArea2(targetPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 0);
                                        MapClass.Instance.RevealArea2(targetPos, pTechno.Ref.LastSightRange, pTechno.Ref.Owner, false, false, false, true, 1);

                                        TechnoTypeData typeData = Ini.GetConfig<TechnoTypeData>(Ini.RulesDependency, section).Data;
                                        // 播放自定义传送动画
                                        Pointer<AnimTypeClass> pAnimType = IntPtr.Zero;
                                        if (typeData.WarpOut.IsNullOrEmpty())
                                        {
                                            pAnimType = RulesClass.Global().WarpOut;
                                        }
                                        else if ("none" != typeData.WarpOut.Trim().ToLower())
                                        {
                                            pAnimType = AnimTypeClass.ABSTRACTTYPE_ARRAY.Find(typeData.WarpOut);
                                        }
                                        if (!pAnimType.IsNull)
                                        {
                                            Pointer<AnimClass> pAnimOut = YRMemory.Create<AnimClass>(pAnimType, sourcePos);
                                            pAnimOut.SetAnimOwner(pTechno);

                                            Pointer<AnimClass> pAnimIn = YRMemory.Create<AnimClass>(pAnimType, targetPos);
                                            pAnimIn.SetAnimOwner(pTechno);
                                        }
                                        // 播放声音
                                        int outSound = pTechno.Ref.Type.Ref.ChronoOutSound;
                                        if (outSound >= 0 || (outSound = RulesClass.Global().ChronoOutSound) >= 0)
                                        {
                                            VocClass.PlayAt(outSound, sourcePos);
                                        }
                                        int inSound = pTechno.Ref.Type.Ref.ChronoInSound;
                                        if (inSound >= 0 || (inSound = RulesClass.Global().ChronoInSound) >= 0)
                                        {
                                            VocClass.PlayAt(inSound, targetPos);
                                        }
                                        // 传送冷冻
                                        int delay = typeData.ChronoMinimumDelay;
                                        if (typeData.ChronoTrigger)
                                        {
                                            // 根据传送距离计算时间
                                            double distance = targetPos.DistanceFrom(sourcePos);
                                            if (distance > typeData.ChronoRangeMinimum)
                                            {
                                                // Logger.Log($"{Game.CurrentFrame} 重算冰冻时间, dist={distance}, RangeMin={typeData.ChronoRangeMinimum}, {typeData.ChronoDistanceFactor}, {typeData.ChronoDelay}");
                                                int factor = Math.Max(typeData.ChronoDistanceFactor, 1);
                                                delay = (int)(distance / factor);
                                            }
                                        }
                                        pTechno.Ref.WarpingOut = true;
                                        teleportTimer.Start(delay);
                                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 自行传送完成，计算冷却，剩余时间 {teleportTimer.GetTimeLeft()}");
                                    }
                                    else if (data.Super)
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
                            if (null != loco)
                            {
                                // 当前帧切换loco后会切回来，而且下一帧才可以获得计时器
                                teleportTimer = loco.ToLocomotionClass<TeleportLocomotionClass>().Ref.Timer;
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 传送完成，获得冷却计时器，剩余时间 {teleportTimer.GetTimeLeft()}");
                            }
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
            CoordStruct targetPos = default;
            ILocomotion loco = pTechno.Convert<FootClass>().Ref.Locomotor;
            // 是否正在移动
            if (loco.Apparent_Speed() > 0)
            {
                targetPos = loco.Destination();
                // 记录下目的地
                pDest = pTechno.Convert<FootClass>().Ref.Destination;
                pFocus = pTechno.Ref.Focus;
            }
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
