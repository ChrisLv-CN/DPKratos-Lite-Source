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
    [GlobalScriptable(typeof(BulletExt))]
    [UpdateAfter(typeof(ProximityScript))]
    public class MissileTrajectoryScript : BulletScriptable
    {
        public MissileTrajectoryScript(BulletExt owner) : base(owner) { }

        private IConfigWrapper<TrajectoryData> _data;
        private TrajectoryData data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<TrajectoryData>(Ini.RulesDependency, section);
                }
                return _data.Data;
            }
        }


        public override void Awake()
        {
            // 非导弹不执行本脚本
            if (pBullet.Ref.Type.Ref.ROT < 1)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType)
        {
            // 高抛导弹
            if (!pBullet.Ref.WeaponType.IsNull && pBullet.Ref.WeaponType.Ref.Lobber)
            {
                if (pBullet.Ref.Velocity.Z < 0)
                {
                    pBullet.Ref.Velocity.Z *= -1;
                }
                pBullet.Ref.Velocity.Z += RulesClass.Global().Gravity;
            }

            // 翻转发射方向
            if (data.ReverseVelocity)
            {
                BulletVelocity velocity = pBullet.Ref.Velocity;
                pBullet.Ref.Velocity *= -1;
                if (!data.ReverseVelocityZ)
                {
                    pBullet.Ref.Velocity.Z = velocity.Z;
                }
            }

            // 晃动的出膛方向
            if (data.ShakeVelocity != 0)
            {
                BulletVelocity velocity = pBullet.Ref.Velocity;
                double shakeX = MathEx.Random.NextDouble() * data.ShakeVelocity;
                double shakeY = MathEx.Random.NextDouble() * data.ShakeVelocity;
                double shakeZ = MathEx.Random.NextDouble();
                pBullet.Ref.Velocity.X *= shakeX;
                pBullet.Ref.Velocity.Y *= shakeY;
                pBullet.Ref.Velocity.Z *= shakeZ;
            }
        }

    }
}
