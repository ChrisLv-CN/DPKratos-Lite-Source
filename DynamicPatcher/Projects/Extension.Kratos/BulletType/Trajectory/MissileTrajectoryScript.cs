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

        public bool IsDecoy;
        public CoordStruct LaunchPos;
        public TimerStruct LifeTimer;
        private int _weaponRange;
        private int WeaponRange
        {
            get
            {
                if (0 == _weaponRange && !pBullet.Ref.WeaponType.IsNull)
                {
                    _weaponRange = pBullet.Ref.WeaponType.Ref.Range;
                }
                return _weaponRange;
            }
        }

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

        private BulletStatusScript status => pBullet.GetStatus();

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

        public override void OnUpdate()
        {
            if (IsDecoy && !pBullet.IsDeadOrInvisible())
            {
                // 检查存活时间
                if (LifeTimer.Expired())
                {
                    if (pBullet.TryGetStatus(out BulletStatusScript status))
                    {
                        status.LifeData.Detonate(true);
                    }
                    else
                    {
                        CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                        pBullet.Ref.Detonate(location);
                        pBullet.Ref.Base.Remove();
                        pBullet.Ref.Base.UnInit();
                    }
                }
                else
                {
                    // 执行热诱弹轨迹变化
                    // Check distance to Change speed and target point
                    int speed = pBullet.Ref.Speed - 5;
                    pBullet.Ref.Speed = speed < 10 ? 10 : speed;
                    if (speed > 10 && LaunchPos.DistanceFrom(pBullet.Ref.Base.Base.GetCoords()) <= WeaponRange)
                    {
                        pBullet.Ref.Base.Location += new CoordStruct(0, 0, 64);
                    }
                }
            }
        }

    }
}
