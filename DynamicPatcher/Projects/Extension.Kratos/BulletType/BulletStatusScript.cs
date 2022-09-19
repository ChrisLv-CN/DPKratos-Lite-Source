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
    public class BulletStatusScript : BulletScriptable
    {
        public BulletStatusScript(BulletExt owner) : base(owner) { }

        public SwizzleablePointer<HouseClass> pSourceHouse = new SwizzleablePointer<HouseClass>(HouseClass.FindSpecial());

        public BulletLifeData LifeData;
        public BulletDamageData DamageData;

        public bool SubjectToGround;

        public override void Awake()
        {
            // Logger.Log($"{Game.CurrentFrame} + Bullet 全局主程，记录下抛射体的所属");
            Pointer<TechnoClass> pShooter = pBullet.Ref.Owner;
            if (!pShooter.IsNull && !pShooter.Ref.Owner.IsNull)
            {
                pSourceHouse.Pointer = pShooter.Ref.Owner;
            }

            ISectionReader reader = Ini.GetSection(Ini.RulesDependency, section);
            // 初始化额外属性
            int health = pBullet.Ref.Base.Health;
            // 抛射体武器伤害为负数或者零时的特殊处理
            if (health < 0)
            {
                health = -health;
            }
            else if (health == 0)
            {
                health = 1; // 武器伤害为0，如[NukeCarrier]
            }
            LifeData = new BulletLifeData(health);
            LifeData.Read(reader);
            // 初始化抛射体的生命信息
            // Logger.Log($"{Game.CurrentFrame} 初始化抛射体 [{section}]{pBullet} 生存属性 {LifeData}");
            // 初始化抛射体的伤害信息
            DamageData = new BulletDamageData(health);
            // Logger.Log($"{Game.CurrentFrame} 初始化抛射体 [{section}]{pBullet} 伤害属性 {DamageData}");
        }

        public override void OnUpdate()
        {
            // 检查抛射体是否已经被摧毁
            if (null != LifeData)
            {
                if (LifeData.IsDetonate)
                {
                    // Logger.Log("抛射体{0}死亡, {1}", OwnerObject, BulletLifeStatus);
                    if (!LifeData.IsHarmless)
                    {
                        CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
                        pBullet.Ref.Detonate(location);
                    }
                    pBullet.Ref.Base.Remove();
                    pBullet.Ref.Base.UnInit();
                    // Logger.Log("抛射体{0}注销", OwnerObject);
                    return;
                }
                // 检查抛射体存活
                if (LifeData.Health <= 0)
                {
                    LifeData.IsDetonate = true;
                    return;
                }
            }
        }

        /// <summary>
        /// 对抛射体造成伤害
        /// </summary>
        /// <param name="damageData"></param>
        /// <param name="Interceptable"></param>
        public void TakeDamage(BulletDamageData damageData, bool Interceptable = false)
        {
            if (null != damageData && null != LifeData && (Interceptable || LifeData.Interceptable))
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 收到伤害{damageData}");
                if (damageData.Eliminate)
                {
                    LifeData.Detonate(damageData.Harmless);
                }
                else
                {
                    LifeData.TakeDamage(damageData.Damage, damageData.Harmless);
                }
            }
        }

    }
}
