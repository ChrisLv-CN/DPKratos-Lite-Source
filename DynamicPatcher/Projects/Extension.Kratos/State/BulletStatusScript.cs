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
    // [UpdateAfter(typeof(AttachEffectScript))]
    public partial class BulletStatusScript : BulletScriptable
    {
        public BulletStatusScript(BulletExt owner) : base(owner) { }

        public SwizzleablePointer<HouseClass> pSourceHouse = new SwizzleablePointer<HouseClass>(HouseClass.FindSpecial());

        public BulletLifeData LifeData;
        public BulletDamageData DamageData;

        public bool SubjectToGround;

        public SwizzleablePointer<ObjectClass> pFakeTarget = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);

        private bool initStateFlag = false;

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

        public override void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType)
        {
            if (!initStateFlag)
            {
                initStateFlag = true;
                // InitState_AttackBeacon();
                InitState_BlackHole();
                InitState_Bounce();
                // InitState_DamageReaction();
                // InitState_Deselect();
                InitState_DestroySelf();
                // InitState_ExtraFire();
                // InitState_FireSuper();
                InitState_GiftBox();
                // InitState_OverrideWeapon();
                // InitState_Paintball();
            }
        }

        public override void OnUpdate()
        {
            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
            OnUpdate_Bounce();
            OnUpdate_DestroySelf();
            // 是否需要检查潜地
            if (!LifeData.IsDetonate && !pBullet.Ref.WH.HasPreImpactAnim())
            {
                if ((SubjectToGround || isBounceSplit) && pBullet.Ref.Base.GetHeight() < 0)
                {
                    // 潜地
                    // Logger.Log($"{Game.CurrentFrame} Arcing 抛射体 [{section}]{pBullet} 潜地，强制爆炸");
                    CoordStruct targetPos = location;
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pTargetCell))
                    {
                        targetPos.Z = pTargetCell.Ref.GetCoordsWithBridge().Z;
                        pBullet.Ref.SetTarget(pTargetCell.Convert<AbstractClass>());
                    }
                    pBullet.Ref.TargetCoords = targetPos;
                    LifeData.Detonate();
                }
                if (!LifeData.IsDetonate && pBullet.AmIArcing() && pBullet.Ref.Base.GetHeight() <= 8)
                {
                    // Arcing 近炸
                    CoordStruct tempSoucePos = location;
                    tempSoucePos.Z = 0;
                    CoordStruct tempTargetPos = pBullet.Ref.TargetCoords;
                    tempTargetPos.Z = 0;
                    // Logger.Log($"{Game.CurrentFrame} 炮弹 [{section}]{pBullet} 贴近地面，距离目标 {tempSoucePos.DistanceFrom(tempTargetPos)}");
                    if (tempSoucePos.DistanceFrom(tempTargetPos) <= 256 + pBullet.Ref.Type.Ref.Acceleration)
                    {
                        // Logger.Log($"{Game.CurrentFrame} 炮弹 [{section}]{pBullet} 距离目标太近，强制爆炸");
                        LifeData.Detonate();
                    }
                }
            }
            // 检查抛射体是否已经被摧毁
            if (null != LifeData)
            {
                if (LifeData.IsDetonate)
                {
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 死亡 {pBullet.Ref.Base.Base.GetCoords()}");
                    if (!LifeData.IsHarmless)
                    {
                        pBullet.Ref.Detonate(location);
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 死亡，调用爆炸 {location}");
                    }
                    // pBullet.Ref.Base.Remove();
                    pBullet.Ref.Base.UnInit();
                    // Logger.Log($"{Game.CurrentFrame} [{section}]{pBullet} 注销");
                    return;
                }
                // 检查抛射体存活
                if (LifeData.Health <= 0)
                {
                    LifeData.IsDetonate = true;
                }
            }
            if (!pBullet.IsDeadOrInvisible() && !LifeData.IsDetonate)
            {
                OnUpdate_BlackHole();
                OnUpdate_GiftBox();
                OnUpdate_RecalculateStatus();
            }
        }

        public override void OnLateUpdate()
        {
            if (!pBullet.IsDeadOrInvisible() && !LifeData.IsDetonate)
            {
                OnLateUpdate_BlackHole();
            }
        }

        /// <summary>
        /// 对抛射体造成伤害
        /// </summary>
        /// <param name="damageData"></param>
        /// <param name="checkInterceptable"></param>
        public void TakeDamage(BulletDamageData damageData, bool checkInterceptable = false)
        {
            if (null != damageData && null != LifeData)
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 收到伤害{damageData}");
                TakeDamage(damageData.Damage, damageData.Eliminate, damageData.Harmless, checkInterceptable);
            }
        }

        public void TakeDamage(int damage, bool eliminate, bool harmless, bool checkInterceptable = false)
        {
            if (null != LifeData && (checkInterceptable || LifeData.Interceptable))
            {
                // Logger.Log($"{Game.CurrentFrame} 抛射体 [{section}]{pBullet} 收到伤害{damage} {eliminate} {harmless} {checkInterceptable}");
                if (eliminate)
                {
                    LifeData.Detonate(harmless);
                }
                else
                {
                    LifeData.TakeDamage(damage, harmless);
                }
            }

        }

        public override void OnDetonate(Pointer<CoordStruct> pCoords, ref bool skip)
        {
            if (!pFakeTarget.IsNull)
            {
                pFakeTarget.Ref.UnInit();
            }
            if (!skip)
            {
                if (skip = OnDetonate_Bounce(pCoords))
                {
                    return;
                }
                if (skip = OnDetonate_GiftBox(pCoords))
                {
                    return;
                }
            }
        }

    }
}
