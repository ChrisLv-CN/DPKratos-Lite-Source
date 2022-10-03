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

        private TechnoStatusScript technoStatus => GameObject.GetComponent<TechnoStatusScript>();
        private List<Trail> trails;

        public override void OnPut(Pointer<CoordStruct> pCoord, DirType dirType)
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

        public override void OnLateUpdate()
        {
            if (null != trails)
            {
                if (!pTechno.IsDeadOrInvisibleOrCloaked() && pTechno.Ref.Base.GetHeight() >= 0)
                {
                    foreach (Trail trail in trails)
                    {
                        // 检查动画尾巴
                        if (trail.Type.Mode == TrailMode.ANIM)
                        {
                            switch (technoStatus.DrivingState)
                            {
                                case DrivingState.Start:
                                case DrivingState.Stop:
                                    trail.SetDrivingState(technoStatus.DrivingState);
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
