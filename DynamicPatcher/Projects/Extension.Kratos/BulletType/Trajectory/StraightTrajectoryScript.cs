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
    public class StraightBullet
    {
        public CoordStruct sourcePos;
        public CoordStruct targetPos;
        public BulletVelocity Velocity;

        public StraightBullet(CoordStruct sourcePos, CoordStruct targetPos, BulletVelocity bulletVelocity)
        {
            this.sourcePos = sourcePos;
            this.targetPos = targetPos;
            this.Velocity = bulletVelocity;
        }
    }

    [Serializable]
    [GlobalScriptable(typeof(BulletExt))]
    [UpdateAfter(typeof(ProximityScript))]
    public class StraightTrajectoryScript : BulletScriptable
    {
        public StraightTrajectoryScript(BulletExt owner) : base(owner) { }

        private BulletStatusScript bulletStatus => GameObject.GetComponent<BulletStatusScript>();
        private ProximityScript proximity => GameObject.GetComponent<ProximityScript>();

        private TrajectoryData trajectoryData => Ini.GetConfig<TrajectoryData>(Ini.RulesDependency, section).Data;

        private StraightBullet straightBullet;

        public override void Awake()
        {
            switch (trajectoryData.SubjectToGround)
            {
                case SubjectToGround.YES:
                    bulletStatus.SubjectToGround = true;
                    break;
                case SubjectToGround.NO:
                    bulletStatus.SubjectToGround = false;
                    break;
                default:
                    bulletStatus.SubjectToGround = pBullet.Ref.Type.Ref.ROT != 1 && !trajectoryData.IsStraight();
                    break;
            }

            // 非直线导弹不执行本脚本
            if (pBullet.Ref.Type.Ref.ROT != 1 && !trajectoryData.IsStraight())
            {
                GameObject.RemoveComponent(this);
                return;
            }

            // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 是直线类型的导弹 {pBullet.Ref.Type.Ref.ROT}");

            // 直线弹道
            CoordStruct sourcePos = pBullet.Ref.SourceCoords;
            CoordStruct targetPos = pBullet.Ref.TargetCoords;

            // 绝对直线，重设目标坐标
            if (trajectoryData.AbsolutelyStraight && !pBullet.Ref.Owner.IsNull)
            {
                // Logger.Log("{0} 绝对直线弹道", pBullet.Ref.Type.Ref.Base.Base.ID);
                double distance = targetPos.DistanceFrom(sourcePos);
                DirStruct facing = pBullet.Ref.Owner.Ref.GetRealFacing().current();
                targetPos = ExHelper.GetFLHAbsoluteCoords(sourcePos, new CoordStruct((int)distance, 0, 0), facing);
                pBullet.Ref.TargetCoords = targetPos;

                // BulletEffectHelper.BlueLine(pBullet.Ref.SourceCoords, pBullet.Ref.TargetCoords, 1, 90);
            }

            // 重设速度
            BulletVelocity velocity = pBullet.RecalculateBulletVelocity(sourcePos, targetPos);
            straightBullet = new StraightBullet(sourcePos, targetPos, velocity);

            // 设置触碰引信
            if (pBullet.Ref.Type.Ref.Proximity)
            {
                // this.Proximity = new Proximity(pBullet.Ref.Owner, pBullet.Ref.Type.Ref.CourseLockDuration);
                proximity.ActiveProximity();
            }
        }

        public override void OnUpdate()
        {
            if (null != straightBullet && !bulletStatus.CaptureByBlackHole)
            {
                // 强制修正速度
                pBullet.Ref.Velocity = straightBullet.Velocity;
            }
        }

    }
}
