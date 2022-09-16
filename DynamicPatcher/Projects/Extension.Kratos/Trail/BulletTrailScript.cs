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
    [GlobalScriptable(typeof(BulletExt))]
    public class BulletTrailScript : BulletScriptable
    {
        public BulletTrailScript(BulletExt owner) : base(owner) { }

        private Pointer<BulletClass> pBullet => Owner.OwnerObject;

        private List<Trail> trails;

        public override void Awake()
        {
            string section = pBullet.Ref.Type.Ref.Base.Base.ID;
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

            if (null != trails)
            {
                CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                CoordStruct forwardLocation = location + pBullet.Ref.Velocity.ToCoordStruct();
                DirStruct bulletFacing = ExHelper.Point2Dir(location, forwardLocation);
                foreach (Trail trail in trails)
                {
                    CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(location, trail.FLH, bulletFacing);
                    trail.SetLastLocation(sourcePos);
                }
            }
        }

        public override void OnUpdate()
        {
            if (null != trails)
            {
                if (pBullet.IsDeadOrInvisible())
                {
                    // 更新位置
                    CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                    CoordStruct forwardLocation = location + pBullet.Ref.Velocity.ToCoordStruct();
                    DirStruct bulletFacing = ExHelper.Point2Dir(location, forwardLocation);
                    foreach (Trail trail in trails)
                    {
                        CoordStruct sourcePos = ExHelper.GetFLHAbsoluteCoords(location, trail.FLH, bulletFacing);
                        trail.UpdateLastLocation(sourcePos);
                    }
                }
                else
                {
                    // 绘制尾巴
                    DrawTrail(pBullet);
                }
            }
        }

        private void DrawTrail(Pointer<BulletClass> pBullet)
        {
            Pointer<HouseClass> pHouse = Owner.ToBulletExt().GameObject.GetComponent<BulletScript>().pHouse;
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
