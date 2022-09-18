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
    public class BulletAntiBulletScript : BulletScriptable
    {
        public BulletAntiBulletScript(BulletExt owner) : base(owner) { }

        private BulletStatusScript bulletStatus => GameObject.GetComponent<BulletStatusScript>();

        public override void Awake()
        {
            // Logger.Log($"{Game.CurrentFrame} 读取抛射体状态");
            // 从发射者身上获取反抛射体设置并更新抛射体伤害设置
            Pointer<TechnoClass> pShooter = bulletStatus.pSourceShooter;
            if (!pShooter.IsNull && pShooter.TryGetComponent<TechnoAntiBulletScript>(out TechnoAntiBulletScript technoAntiBullet))
            {
                AntiBulletData antiBulletData = technoAntiBullet.AntiBulletData;
                if (null != antiBulletData)
                {
                    bulletStatus.DamageData.Eliminate = antiBulletData.OneShotOneKill;
                    bulletStatus.DamageData.Harmless = antiBulletData.Harmless;
                    // Logger.Log($"{Game.CurrentFrame} 重设抛射体 [{section}]{pBullet} 的伤害属性 {bulletStatus.DamageData} 由射手 [{pShooter.Ref.Type.Ref.Base.Base.ID}]{pShooter} 发射");
                }
            }
        }

        public override void OnDetonate(Pointer<CoordStruct> location)
        {
            // TODO 应该检索范围内的所有抛射体，并对其造成伤害

            // 检查抛射体是否命中了其他抛射体，并对其造成伤害
            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            if (pTarget.CastIf<BulletClass>(AbstractType.Bullet, out Pointer<BulletClass> pTargetBullet)
                && HitTarget(location.Data, pTargetBullet, out BulletStatusScript targetStatus))
            {
                // 对目标抛射体造成伤害
                targetStatus.TakeDamage(bulletStatus.DamageData);
            }
        }

        private bool HitTarget(CoordStruct location, Pointer<BulletClass> pTarget, out BulletStatusScript targetStatus)
        {
            if (!pTarget.IsNull)
            {
                // 爆炸的位置距离目标足够的近
                if (pBullet.Ref.Type.Ref.Inviso || location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords()) <= pBullet.Ref.Type.Ref.Arm)
                {
                    return pTarget.TryGetStatus(out targetStatus);
                }
            }
            targetStatus = null;
            return false;
        }

    }
}