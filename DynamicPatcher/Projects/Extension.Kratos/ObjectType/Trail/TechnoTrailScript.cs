using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    public enum DrivingState
    {
        Moving = 0, Stand = 1, Start = 2, Stop = 3
    }

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class TechnoTrailScript : TechnoScriptable
    {
        public TechnoTrailScript(TechnoExt owner) : base(owner) { }

        private Mission lastMission;
        private DrivingState drivingState;

        private List<Trail> trails;

        public override void OnPut(CoordStruct coord, short dirType)
        {
            if (TrailHelper.TryGetTrails(section, out List<Trail> trails))
            {
                this.trails = trails;
            }
        }

        public override void OnRemove()
        {
            this.trails = null;
        }

        public override void OnUpdate()
        {
            if (null != trails)
            {
                if (!pTechno.IsDead())
                {
                    Mission mission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
                    switch (mission)
                    {
                        case Mission.Move:
                        case Mission.AttackMove:
                            // 上一次任务不是这两个说明是起步
                            if (Mission.Move != lastMission && Mission.AttackMove != lastMission)
                            {
                                drivingState = DrivingState.Start;
                            }
                            else
                            {
                                drivingState = DrivingState.Moving;
                            }
                            break;
                        default:
                            // 上一次任务如果是Move或者AttackMove说明是刹车
                            if (Mission.Move == lastMission || Mission.AttackMove == lastMission)
                            {
                                drivingState = DrivingState.Stop;
                            }
                            else
                            {
                                drivingState = DrivingState.Stand;
                            }
                            break;
                    }
                    lastMission = mission;
                }
            }
        }

        public override void OnLateUpdate()
        {
            if (null != trails)
            {
                if (!pTechno.IsDeadOrInvisibleOrCloaked())
                {
                    foreach (Trail trail in trails)
                    {
                        // 检查动画尾巴
                        if (trail.Type.Mode == TrailMode.ANIM)
                        {
                            switch (drivingState)
                            {
                                case DrivingState.Start:
                                case DrivingState.Stop:
                                    trail.SetDrivingState(drivingState);
                                    break;
                            }
                        }
                        CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(pTechno, trail.FLH, trail.IsOnTurret);
                        trail.DrawTrail(pTechno.Ref.Owner, sourcePos);
                    }
                }
                else
                {
                    // 更新坐标
                    foreach (Trail trail in trails)
                    {
                        CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(pTechno, trail.FLH, trail.IsOnTurret);
                        trail.UpdateLastLocation(sourcePos);
                    }

                }
            }
        }

    }
}
