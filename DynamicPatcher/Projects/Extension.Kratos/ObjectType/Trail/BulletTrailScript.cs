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
            if (null != trails && !pBullet.IsDeadOrInvisible() && pBullet.Ref.Base.GetHeight() >= 0)
            {
                Pointer<HouseClass> pHouse = pBullet.GetStatus().pSourceHouse;

                CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                DirStruct bulletFacing = pBullet.Facing(location);
                foreach (Trail trail in trails)
                {
                    CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(location, trail.FLH, bulletFacing);
                    trail.DrawTrail(pHouse, sourcePos);
                }
            }
        }

    }
}
