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

        private TrajectoryData trajectoryData => Ini.GetConfig<TrajectoryData>(Ini.RulesDependency, section).Data;


        public override void Awake()
        {
            // 非导弹不执行本脚本
            if (pBullet.Ref.Type.Ref.ROT < 1)
            {
                GameObject.RemoveComponent(this);
                return;
            }
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
            if (trajectoryData.ReverseVelocity)
            {
                BulletVelocity velocity = pBullet.Ref.Velocity;
                pBullet.Ref.Velocity *= -1;
                if (!trajectoryData.ReverseVelocityZ)
                {
                    pBullet.Ref.Velocity.Z = velocity.Z;
                }
            }

            // 晃动的出膛方向
            if (trajectoryData.ShakeVelocity != 0)
            {
                BulletVelocity velocity = pBullet.Ref.Velocity;
                double shakeX = MathEx.Random.NextDouble() * trajectoryData.ShakeVelocity;
                double shakeY = MathEx.Random.NextDouble() * trajectoryData.ShakeVelocity;
                double shakeZ = MathEx.Random.NextDouble();
                pBullet.Ref.Velocity.X *= shakeX;
                pBullet.Ref.Velocity.Y *= shakeY;
                pBullet.Ref.Velocity.Z *= shakeZ;
            }
        }

    }
}
