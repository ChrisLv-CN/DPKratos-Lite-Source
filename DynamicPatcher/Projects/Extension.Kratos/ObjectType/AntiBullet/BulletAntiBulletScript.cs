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
            Pointer<TechnoClass> pShooter = pBullet.Ref.Owner;
            if (!pShooter.IsNull && pShooter.TryGetComponent<TechnoAntiBulletScript>(out TechnoAntiBulletScript technoAntiBullet))
            {
                AntiBulletData antiBulletData = technoAntiBullet.Data;
                if (null != antiBulletData)
                {
                    bulletStatus.DamageData.Eliminate = antiBulletData.OneShotOneKill;
                    bulletStatus.DamageData.Harmless = antiBulletData.Harmless;
                    // Logger.Log($"{Game.CurrentFrame} 重设抛射体 [{section}]{pBullet} 的伤害属性 {bulletStatus.DamageData} 由射手 [{pShooter.Ref.Type.Ref.Base.Base.ID}]{pShooter} 发射");
                }
            }

            Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
            // 若目标非可摧毁的抛射体则不执行本逻辑
            if (pTarget.IsNull || !pTarget.CastIf<BulletClass>(AbstractType.Bullet, out Pointer<BulletClass> pTargetBullet)
                || !pTargetBullet.GetStatus().LifeData.Interceptable)
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnDetonate(Pointer<CoordStruct> location, ref bool skip)
        {
            // 检索范围内的所有抛射体，并对其造成伤害
            Pointer<WarheadTypeClass> pWH = pBullet.Ref.WH;
            if (pWH.Ref.CellSpread > 0)
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 爆炸，检索范围内的抛射体，检索范围 {pWH.Ref.CellSpread}");
                BulletClass.Array.FindObject((pTarget) => {
                    CanAffectAndDamageBullet(pTarget, pWH);
                    return false;
                }, location.Data, pWH.Ref.CellSpread, 0, false, bulletStatus.pSourceHouse);
            }
            else
            {
                // 检查与预定目标是否足够近
                Pointer<AbstractClass> pTagret = pBullet.Ref.Target;
                if (!pTagret.IsNull)
                {
                    CoordStruct targetPos = pTagret.Ref.GetCoords();
                    if (location.Ref.DistanceFrom(targetPos) <= pBullet.Ref.Type.Ref.Arm + 256
                        && pTagret.CastIf<BulletClass>(AbstractType.Bullet, out Pointer<BulletClass> pTargetBullet))
                    {
                        CanAffectAndDamageBullet(pTargetBullet, pWH);
                    }
                }
            }
        }

        private void CanAffectAndDamageBullet(Pointer<BulletClass> pTarget, Pointer<WarheadTypeClass> pWH)
        {
            if (!pTarget.IsNull && pTarget != pBullet
                && pTarget.TryGetStatus(out BulletStatusScript targetStatus)
                && pWH.CanAffectHouse(bulletStatus.pSourceHouse, targetStatus.pSourceHouse))
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 对抛射体 [{pTarget.Ref.Type.Ref.Base.Base.ID}]{pTarget} 制造伤害");
                targetStatus.TakeDamage(bulletStatus.DamageData);
            }
        }
    }
}
