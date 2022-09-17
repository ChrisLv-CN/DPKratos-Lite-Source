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
    [GlobalScriptable(typeof(BulletExt))]
    [UpdateAfter(typeof(BulletStatusScript))]
    public class BulletTrailScript : BulletScriptable
    {
        public BulletTrailScript(BulletExt owner) : base(owner) { }

        private List<Trail> trails;

        public override void Awake()
        {
            // Logger.Log($"{Game.CurrentFrame} BulletTrailScript Awake");
            if (TrailHelper.TryGetTrails(section, out List<Trail> trails))
            {
                this.trails = trails;
            }
        }

        public override void OnLateUpdate()
        {
            if (null != trails && !pBullet.IsDeadOrInvisible())
            {
                Pointer<HouseClass> pHouse = Owner.ToBulletExt().GameObject.GetComponent<BulletStatusScript>().pSourceHouse;

                CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                CoordStruct forwardLocation = location + pBullet.Ref.Velocity.ToCoordStruct();
                DirStruct bulletFacing = ExHelper.Point2Dir(location, forwardLocation);
                foreach (Trail trail in trails)
                {
                    CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(location, trail.FLH, bulletFacing);
                    trail.DrawTrail(pHouse, sourcePos);
                }
            }
        }

    }
}
