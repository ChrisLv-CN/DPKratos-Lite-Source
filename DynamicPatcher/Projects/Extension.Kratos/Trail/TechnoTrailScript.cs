using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.Utilities;
using Extension.INI;

namespace Extension.Script
{

    [Serializable]
    public enum DrivingState
    {
        Moving = 0, Stand = 1, Start = 2, Stop = 3
    }

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    public class TechnoTrailScript : TechnoScriptable
    {
        public TechnoTrailScript(TechnoExt owner) : base(owner) { }

        private Pointer<TechnoClass> pTechno => Owner.OwnerObject;

        private Mission lastMission;
        private DrivingState drivingState;

        private List<Trail> trails;

        public override void Awake()
        {

            string section = pTechno.Ref.Type.Ref.Base.Base.ID;
            // Logger.Log($"{Game.CurrentFrame} 多类型脚本 挂载 到 {section} 我是 {pObject.Ref.Base.WhatAmI()}");

            string image = Ini.GetSection(Ini.RulesDependency, section).Get<string>("Image");
            if (!string.IsNullOrEmpty(image))
            {
                section = image;
            }
            // 获取尾巴
            ISectionReader reader = Ini.GetSection(Ini.ArtDependency, section);
            // 读取没有写数字的
            string s = "Trail.Type";
            string trailType = reader.Get<string>(s);
            if (!string.IsNullOrEmpty(trailType) && Ini.HasSection(Ini.ArtDependency, trailType))
            {
                IConfigWrapper<TrailType> type = Ini.GetConfig<TrailType>(Ini.ArtDependency, trailType);
                Trail trail = new Trail(type);
                trail.Read(reader, "Trail.");

                trails = new List<Trail>();
                trails.Add(trail);
            }
            // 读取0-11的
            for (int i = 0; i < 12; i++)
            {
                s = "Trail" + i + ".Type";
                trailType = reader.Get<string>(s);
                if (!string.IsNullOrEmpty(trailType) && Ini.HasSection(Ini.ArtDependency, trailType))
                {
                    IConfigWrapper<TrailType> type = Ini.GetConfig<TrailType>(Ini.ArtDependency, trailType);
                    Trail trail = new Trail(type);
                    trail.Read(reader, "Trail" + i + ".");
                    if (null == trails)
                    {
                        trails = new List<Trail>();
                    }
                    trails.Add(trail);
                }
            }

            // Logger.Log($"{Game.CurrentFrame} 类型 {section} 有 {(null != trails ? trails.Count : 0)} 条 尾巴");
        }

        public override void OnPut(CoordStruct coord, short dirType)
        {

        }

        public override void OnUpdate()
        {
            if (null != trails)
            {
                Pointer<TechnoClass> pTechno = Owner.OwnerObject;
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
                if (pTechno.IsDeadOrInvisibleOrCloaked())
                {
                    // 更新位置
                }
                else
                {
                    // 绘制尾巴
                    DrawTrail(pTechno);
                }
            }
        }

        private void OnTechnoUpdate()
        {
            Pointer<TechnoClass> pTechno = Owner.OwnerObject.Convert<TechnoClass>();
            DrawTrail(pTechno);

        }

        private void DrawTrail(Pointer<TechnoClass> pTechno)
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

        public override void OnRemove()
        {

        }

    }
}
