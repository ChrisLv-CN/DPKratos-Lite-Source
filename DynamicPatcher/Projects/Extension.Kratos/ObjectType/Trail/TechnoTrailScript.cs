using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
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
    // [UpdateAfter(typeof(TechnoStatusScript))]
    public class TechnoTrailScript : TechnoScriptable
    {
        public TechnoTrailScript(TechnoExt owner) : base(owner) { }

        private TechnoStatusScript technoStatus => GameObject.GetComponent<TechnoStatusScript>();
        private List<Trail> trails;

        public override void Awake()
        {
            if (!pTechno.Convert<AbstractClass>().Ref.AbstractFlags.HasFlag(AbstractFlags.Foot))
            {
                GameObject.RemoveComponent(this);
                return;
            }
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void OnUnInit()
        {
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public void OnTransform(object sender, EventArgs args)
        {
            Pointer<TechnoClass> pTarget = ((TechnoTypeChangeEventArgs)args).pTechno;
            if (!pTarget.IsNull && pTarget == pTechno)
            {
                SetupTrails();
            }
        }

        public void SetupTrails(List<Trail> trails = null)
        {
            if ((null != trails && trails.Any()) || TrailHelper.TryGetTrails(section, out trails))
            {
                this.trails = trails;
            }
            else
            {
                this.trails = null;
            }
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, ref DirType dirType)
        {
            if (null == this.trails)
            {
                SetupTrails();
            }
        }

        public override void OnRemove()
        {
            this.trails = null;
        }

        public override void OnLateUpdate()
        {
            if (null != trails && trails.Any())
            {
                if (!pTechno.IsDeadOrInvisibleOrCloaked() && pTechno.Ref.Base.GetHeight() >= 0)
                {
                    foreach (Trail trail in trails)
                    {
                        // ??????????????????
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
                        CoordStruct sourcePos = FLHHelper.GetFLHAbsoluteCoords(pTechno, trail.FLH, trail.IsOnTurret);
                        trail.DrawTrail(pTechno.Ref.Owner, sourcePos);
                    }
                }
                else
                {
                    // ????????????
                    foreach (Trail trail in trails)
                    {
                        CoordStruct sourcePos = FLHHelper.GetFLHAbsoluteCoords(pTechno, trail.FLH, trail.IsOnTurret);
                        trail.UpdateLastLocation(sourcePos);
                    }

                }
            }
        }

    }
}
