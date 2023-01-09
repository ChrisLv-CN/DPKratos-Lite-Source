using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(BulletExt))]
    // [UpdateAfter(typeof(AttachEffectScript))]
    public partial class BulletStatusScript : BulletScriptable
    {

        public static List<BulletExt> TargetAircraftBullets = new List<BulletExt>();

        public BulletStatusScript(BulletExt owner) : base(owner) { }

        private TechnoExt _sourceExt;
        public Pointer<TechnoClass> pSource
        {
            set
            {
                if (!value.IsNull)
                {
                    _sourceExt = TechnoExt.ExtMap.Find(value);
                }
            }
            get
            {
                if (null != _sourceExt)
                {
                    return _sourceExt.OwnerObject;
                }
                return pBullet.Ref.Owner;
            }
        }

        private HouseExt _sourceHouse;
        public Pointer<HouseClass> pSourceHouse
        {
            set
            {
                if (!value.IsNull)
                {
                    _sourceHouse = HouseExt.ExtMap.Find(value);
                }
            }
            get
            {
                if (null != _sourceHouse)
                {
                    return _sourceHouse.OwnerObject;
                }
                return HouseClass.FindSpecial();
            }
        }

        public BulletLifeData LifeData;
        public BulletDamageData DamageData;

        public bool SubjectToGround;

        private IConfigWrapper<TrajectoryData> _trajectoryData;
        private TrajectoryData trajectoryData
        {
            get
            {
                if (null == _trajectoryData)
                {
                    _trajectoryData = Ini.GetConfig<TrajectoryData>(Ini.RulesDependency, section);
                }
                return _trajectoryData.Data;
            }
        }

        private bool hasFakeTarget;
        private SwizzleablePointer<ObjectClass> pFakeTarget = new SwizzleablePointer<ObjectClass>(IntPtr.Zero);

        private bool isInvisible;
        public bool IsInvisible
        {
            get
            {
                if (initStateFlag)
                {
                    return isInvisible;
                }
                return isInvisible = pBullet.Ref.Type.Ref.Inviso;
            }
        }
        private BulletType _bulletType;
        private BulletType bulletType
        {
            get
            {
                if (default == _bulletType)
                {
                    _bulletType = pBullet.WhatTypeAmI();
                    if (_bulletType != BulletType.ROCKET && trajectoryData.IsStraight())
                    {
                        _bulletType = BulletType.ROCKET;
                    }
                }
                return _bulletType;
            }
        }
        private bool isArcing => bulletType == BulletType.ARCING;
        private bool isMissile => bulletType == BulletType.MISSILE;
        private bool isRocket => bulletType == BulletType.ROCKET;
        private bool isBomb => bulletType == BulletType.BOMB;


        private bool initStateFlag = false;

        public static void Clear(object sender, EventArgs args)
        {
            TargetAircraftBullets.Clear();
        }

        public void SetFakeTarget(Pointer<ObjectClass> pFakeTarget)
        {
            hasFakeTarget = true;
            EventSystem.PointerExpire.AddTemporaryHandler(EventSystem.PointerExpire.AnnounceExpiredPointerEvent, ClearFakeTargetHandler);
        }

        private void ClearFakeTargetHandler(object sender, EventArgs e)
        {
            if (hasFakeTarget)
            {
                AnnounceExpiredPointerEventArgs args = (AnnounceExpiredPointerEventArgs)e;
                Pointer<AbstractClass> pAbstract = args.ExpiredPointer;
                if (pAbstract == pFakeTarget.Pointer.Convert<AbstractClass>())
                {
                    pFakeTarget.Pointer = IntPtr.Zero;
                }
            }
        }

        public override void Awake()
        {
            // Logger.Log($"{Game.CurrentFrame} + Bullet 全局主程，记录下抛射体的所属");
            Pointer<TechnoClass> pShooter = pBullet.Ref.Owner;
            if (!pShooter.IsNull)
            {
                pSource = pShooter;

                Pointer<HouseClass> pShooterHouse = IntPtr.Zero;
                if (!(pShooterHouse = pShooter.Ref.Owner).IsNull)
                {
                    pSourceHouse = pShooterHouse;
                }
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
            // 初始化碰撞地面设置
            switch (trajectoryData.SubjectToGround)
            {
                case SubjectToGroundType.YES:
                    SubjectToGround = true;
                    break;
                case SubjectToGroundType.NO:
                    SubjectToGround = false;
                    break;
                default:
                    // Arcing和直线导弹可以穿透地面
                    SubjectToGround = !isArcing && !isRocket && !trajectoryData.IsStraight();
                    break;
            }
        }

        public override void OnPut(Pointer<CoordStruct> pLocation, ref DirType dirType)
        {
            if (!initStateFlag)
            {
                initStateFlag = true;
                InitState_BlackHole();
                InitState_Bounce();
                InitState_DestroySelf();
                InitState_GiftBox();
                InitState_Proximity();
                // 弹道初始化
                if (isMissile)
                {
                    InitState_ECM();
                    InitState_Trajectory_Missile();
                }
                else if (isRocket)
                {
                    InitState_Trajectory_Straight();
                }
                // Logger.Log($"{Game.CurrentFrame} 发射抛射体[{section}]{pBullet} 类型是 {bulletType}");
            }
            Pointer<AbstractClass> pTarget = IntPtr.Zero;
            if (isMissile && !(pTarget = pBullet.Ref.Target).IsNull && pTarget.Ref.WhatAmI() == AbstractType.Aircraft)
            {
                TargetAircraftBullets.Add(Owner);
            }
        }

        public override void OnUnInit()
        {
            TargetAircraftBullets.Remove(Owner);
            if (hasFakeTarget)
            {
                EventSystem.PointerExpire.RemoveTemporaryHandler(EventSystem.PointerExpire.AnnounceExpiredPointerEvent, ClearFakeTargetHandler);
            }
        }

        public override void OnUpdate()
        {
            // 弹道相关逻辑
            if (isArcing)
            {
                OnUpdate_Trajectory_Arcing();
                OnUpdate_Trajectory_Bounce();
            }
            else if (isRocket)
            {
                OnUpdate_Trajectory_Straight();
            }
            OnUpdate_Trajectory_Decroy();

            OnUpdate_DestroySelf();

            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
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
                if (!LifeData.IsDetonate && isArcing && pBullet.Ref.Base.GetHeight() <= 8)
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
                if (isMissile)
                {
                    OnUpdate_ECM();
                }
                OnUpdate_GiftBox();
                OnUpdate_RecalculateStatus();
            }
        }

        public override void OnLateUpdate()
        {
            if (!pBullet.IsDeadOrInvisible() && !LifeData.IsDetonate)
            {
                CoordStruct sourcePos = pBullet.Ref.Base.Base.GetCoords();
                OnLateUpdate_BlackHole(ref sourcePos); // 黑洞会更新位置，要第一个执行
                OnLateUpdate_Proximity(ref sourcePos);
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
