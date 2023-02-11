using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Components;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    // [UpdateAfter(typeof(AttachEffectScript))]
    public partial class TechnoStatusScript : TechnoScriptable
    {
        public TechnoStatusScript(TechnoExt owner) : base(owner) { }

        public bool DisableVoxelCache;
        public float VoxelShadowScaleInAir;

        public bool DisableSelectVoice;

        public DrivingState DrivingState;
        private Mission lastMission;

        private IConfigWrapper<TechnoTypeData> _typeData;
        private TechnoTypeData typeData
        {
            get
            {
                if (null == _typeData)
                {
                    _typeData = Ini.GetConfig<TechnoTypeData>(Ini.RulesDependency, section);
                }
                return _typeData.Data;
            }
        }

        private AbstractType _absType;
        private AbstractType absType
        {
            get
            {
                if (default == _absType)
                {
                    _absType = pTechno.Ref.Base.Base.WhatAmI();
                }
                return _absType;
            }
        }

        private bool isBuilding => absType == AbstractType.Building;
        private bool isInfantry => absType == AbstractType.Infantry;
        private bool isUnit => absType == AbstractType.Unit;
        private bool isAircraft => absType == AbstractType.Aircraft;

        private bool isVoxel;
        private bool isFearless;

        private LocoType _locoType;
        private LocoType locoType
        {
            get
            {
                if (default == _locoType)
                {
                    _locoType = pTechno.WhatLocoType();
                }
                return _locoType;
            }
        }

        private bool isFly => locoType == LocoType.Fly;
        private bool isJumpjet => locoType == LocoType.Jumpjet;
        private bool isShip => locoType == LocoType.Ship;

        private bool initStateFlag = false;

        private CoordStruct location;
        private bool isMoving;

        public static void Clear(object sender, EventArgs args)
        {
            Clear_StandUnit();
            Clear_BaseNormal();
        }

        /// <summary>
        /// 继承除了GiftBox之外的状态
        /// </summary>
        /// <param name="heir"></param>
        private void InheritedStatsTo(TechnoStatusScript heir)
        {
            // 炒粉
            heir.AttackBeaconState = this.AttackBeaconState;
            this.AttackBeaconState = new AttackBeaconState();
            // 黑洞
            heir.BlackHoleState = this.BlackHoleState;
            this.BlackHoleState = new BlackHoleState();
            // Buff
            heir.CrateBuffState = this.CrateBuffState;
            this.CrateBuffState = new State<CrateBuffData>();
            // 伤害响应
            heir.DamageReactionState = this.DamageReactionState;
            this.DamageReactionState = new DamageReactionState();
            // 不可选择
            heir.DeselectState = this.DeselectState;
            this.DeselectState = new State<DeselectData>();
            // 自毁
            heir.DestroySelfState = this.DestroySelfState;
            this.DestroySelfState = new DestroySelfState();
            // 关闭武器
            heir.DisableWeaponState = this.DisableWeaponState;
            this.DisableWeaponState = new State<DisableWeaponData>();
            // 额外武器
            heir.ExtraFireState = this.ExtraFireState;
            this.ExtraFireState = new ExtraFireState();
            // 发射超武
            heir.FireSuperState = this.FireSuperState;
            this.FireSuperState = new State<FireSuperData>();
            // 覆盖武器
            heir.OverrideWeaponState = this.OverrideWeaponState;
            this.OverrideWeaponState = new OverrideWeaponState();
            // 分散
            heir.ScatterState = this.ScatterState;
            this.ScatterState = new State<ScatterData>();
            // 传送
            heir.TeleportState = this.TeleportState;
            this.TeleportState = new TeleportState();
            // 染色弹
            heir.PaintballState = this.PaintballState;
            this.PaintballState = new PaintballState();
        }

        public override void Awake()
        {
            Awake_Transform();

            this.VoxelShadowScaleInAir = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual).Get("VoxelShadowScaleInAir", 2f);
            if (isInfantry)
            {
                isFearless = pTechno.Convert<InfantryClass>().Ref.Type.Ref.Fearless;
            }
            isVoxel = pTechno.Ref.IsVoxel();
        }

        private void InitState()
        {
            // TODO 初始化状态机
            InitState_AttackBeacon();
            InitState_AutoFireAreaWeapon();
            InitState_BlackHole();
            InitState_DamageReaction();
            InitState_Deselect();
            InitState_DestroySelf();
            InitState_ExtraFire();
            InitState_FireSuper();
            InitState_Freeze();
            InitState_GiftBox();
            InitState_OverrideWeapon();
            InitState_Paintball();
            InitState_Scatter();
            InitState_Pump();
            InitState_Teleport();
            InitState_VirtualUnit();
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, ref DirType dirType)
        {
            OnPut_BaseNormal(pCoord, dirType);
            OnPut_StandUnit(pCoord, dirType);

            if (!initStateFlag)
            {
                initStateFlag = true;
                InitState();
            }
            OnPut_TurretAngle(pCoord, dirType);
        }

        public override void OnUpdate()
        {
            location = pTechno.Ref.Base.Base.GetCoords();
            OnUpdate_DestroySelf();
            if (!pTechno.IsDead())
            {
                Mission mission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
                switch (mission)
                {
                    case Mission.Move:
                    case Mission.AttackMove:
                        // 上一次任务不是这两个说明是起步
                        if (Mission.Move != lastMission && Mission.AttackMove != lastMission)
                        {
                            DrivingState = DrivingState.Start;
                        }
                        else
                        {
                            DrivingState = DrivingState.Moving;
                        }
                        break;
                    default:
                        // 上一次任务如果是Move或者AttackMove说明是刹车
                        if (Mission.Move == lastMission || Mission.AttackMove == lastMission)
                        {
                            DrivingState = DrivingState.Stop;
                        }
                        else
                        {
                            DrivingState = DrivingState.Stand;
                        }
                        break;
                }
                if (!isBuilding)
                {
                    Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
                    isMoving = pFoot.Ref.Locomotor.Is_Moving() && pFoot.Ref.GetCurrentSpeed() > 0;
                }
                OnUpdate_Transfrom();

                OnUpdate_AttackBeacon();
                OnUpdate_AutoFireAreaWeapon();
                OnUpdate_BlackHole();
                OnUpdate_CrawlingFLH();
                OnUpdate_DamageReaction();
                OnUpdate_DamageText();
                OnUpdate_Deselect();
                OnUpdate_Freeze();
                OnUpdate_GiftBox();
                OnUpdate_Paintball();
                OnUpdate_Passenger();
                OnUpdate_Pump();
                OnUpdate_Scatter();
                OnUpdate_Teleport();
                OnUpdate_TurretAngle();
            }
        }

        public override void OnLateUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                this.lastMission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
            }
        }

        public override void OnWarpUpdate()
        {
            OnWarpUpdate_DestroySelf_Stand();
        }

        public override void OnTemporalUpdate(Pointer<TemporalClass> pTemporal)
        {
            if (!pTemporal.IsNull && !pTemporal.Ref.Owner.IsNull)
            {
                // check range
                Pointer<TechnoClass> pAttacker = pTemporal.Ref.Owner;
                Pointer<AbstractClass> pTarget = pTechno.Convert<AbstractClass>();
                int weaponIdx = pAttacker.Ref.SelectWeapon(pTarget);
                if (weaponIdx < 0 || !pAttacker.Ref.IsCloseEnough(pTechno.Convert<AbstractClass>(), weaponIdx))
                {
                    pTemporal.Ref.LetGo();
                }
            }
        }

        public override void OnRemove()
        {
            OnRemove_BaseNormal();
            OnRemove_StandUnit();
        }

        public override void OnUnInit()
        {
            OnRemove_BaseNormal();
            OnRemove_StandUnit();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                WarheadTypeData warheadTypeData = pWH.GetData();
                // 弹头效果
                if (pWH.CanAffectHouse(pTechno.Ref.Owner, pAttackingHouse, warheadTypeData) && pTechno.CanDamageMe(pDamage.Ref, distanceFromEpicenter, pWH))
                {
                    // 清理目标弹头
                    if (!pTechno.Ref.Target.IsNull)
                    {
                        if (warheadTypeData.ClearTarget)
                        {
                            pTechno.ClearAllTarget();
                        }
                    }
                    // 清理伪装弹头
                    if (warheadTypeData.ClearDisguise && pTechno.Ref.Base.IsDisguised())
                    {
                        pTechno.Ref.Disguised = false;
                    }
                    // 欠揍弹头
                    if (warheadTypeData.Lueluelue && !pAttacker.IsNull && pAttacker.CastToTechno(out Pointer<TechnoClass> pTargetTechno) && pTechno.CanAttack(pTargetTechno))
                    {
                        pTechno.Ref.SetTarget(pAttacker.Convert<AbstractClass>());
                        pTechno.Ref.BaseMission.ForceMission(Mission.Attack);
                    }
                    // 强制任务弹头
                    Mission forceMission = warheadTypeData.ForceMission;
                    if (forceMission != Mission.None)
                    {
                        pTechno.Ref.BaseMission.ForceMission(forceMission);
                    }
                    // 经验修改弹头
                    if ((warheadTypeData.ExpCost != 0 || warheadTypeData.ExpLevel != ExpLevel.None) && pTechno.Ref.Type.Ref.Trainable)
                    {
                        switch (warheadTypeData.ExpLevel)
                        {
                            case ExpLevel.Rookie:
                                pTechno.Ref.Veterancy.SetRookie();
                                break;
                            case ExpLevel.Veteran:
                                pTechno.Ref.Veterancy.SetVeteran();
                                break;
                            case ExpLevel.Elite:
                                pTechno.Ref.Veterancy.SetElite();
                                break;
                            default:
                                int cost = pTechno.Ref.Type.Ref.Base.GetActualCost(pTechno.Ref.Owner);
                                pTechno.Ref.Veterancy.Add(cost, warheadTypeData.ExpCost);
                                break;
                        }
                    }
                }
                OnReceiveDamage_DamageReaction(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse, warheadTypeData);
                OnReceiveDamage_Stand(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse, warheadTypeData);
            }
        }

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (damageState == DamageState.NowDead)
            {
                // 被打死时读取弹头设置
                OnReceiveDamage2_DestroyAnim(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
            }
            OnReceiveDamage2_BlackHole(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
            OnReceiveDamage2_DamageText(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
            OnReceiveDamage2_GiftBox(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamageDestroy()
        {
            OnReceiveDamageDestroy_BaseNormal();
            OnReceiveDamageDestroy_Transform();
            OnReceiveDamageDestroy_StandUnit();

            OnReceiveDamageDestroy_GiftBox();
        }

        public override void OnRegisterDestruction(Pointer<TechnoClass> pKiller, int cost, ref bool skip)
        {
            if (!skip)
            {
                if (skip = OnRegisterDestruction_StandUnit(pKiller, cost))
                {
                    return;
                }
            }
        }

        public override void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire)
        {
            if (!ceaseFire)
            {
                if (ceaseFire = CanFire_DisableWeapon(pTarget, pWeapon))
                {
                    return;
                }
                if (ceaseFire = CanFire_Passenger(pTarget, pWeapon))
                {
                    return;
                }
                // if (ceaseFire = CanFire_TurretAngle(pTarget, pWeapon))
                // {
                //     return;
                // }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            OnFire_AttackBeacon_Recruit(pTarget, weaponIndex);
            OnFire_ExtraFire(pTarget, weaponIndex);
            OnFire_FireSuper(pTarget, weaponIndex);
            OnFire_OverrideWeapon(pTarget, weaponIndex);
            OnFire_RockerPitch(pTarget, weaponIndex);
        }

        public override void OnSelect(ref bool selectable)
        {
            if (!(selectable = OnSelect_VirtualUnit()))
            {
                return;
            }
            if (!(selectable = OnSelect_Deselect()))
            {
                return;
            }
        }

        public void OnTransform()
        {
            _autoFireAreaWeaponData = null;
            _crawlingFLHData = null;
            _destroyAnimData = null;
            _healthTextTypeData = null;
            _passengersData = null;
            _turretAngleData = null;
            _typeData = null;
            _fireFLHOnBodyData = null;
            _fireFLHOnTargetData = null;
            _spawnData = null;
            // 重新初始化状态机
            InitState();
        }

        /// <summary>
        /// 查找爆炸位置的替身或者虚拟单位并对其造成伤害
        /// </summary>
        /// <param name="location"></param>
        /// <param name="damage"></param>
        /// <param name="pAttacker"></param>
        /// <param name="pWH"></param>
        /// <param name="pAttackingHouse"></param>
        /// <param name="exclue"></param>
        public static void FindAndDamageStandOrVUnit(CoordStruct location, int damage, Pointer<ObjectClass> pAttacker,
           Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pAttackingHouse, Pointer<ObjectClass> exclude = default)
        {
            double spread = pWH.Ref.CellSpread * 256;
            Dictionary<TechnoExt, DamageGroup> targets = new Dictionary<TechnoExt, DamageGroup>();
            // 搜索符合条件的替身
            // Logger.Log($"{Game.CurrentFrame}, 替身列表里有 {TechnoStatusScript.StandArray.Count()} 个记录");
            foreach (KeyValuePair<TechnoExt, StandData> stand in TechnoStatusScript.StandArray)
            {
                if (!stand.Key.OwnerObject.IsNull && null != stand.Value)
                {
                    Pointer<TechnoClass> pStand = stand.Key.OwnerObject;
                    StandData standData = stand.Value;
                    if (!standData.Immune && pStand.Convert<ObjectClass>() != exclude
                        && CheckAndMarkTarget(pStand, spread, location, damage, pAttacker, pWH, pAttackingHouse, out DamageGroup damageGroup))
                    {
                        targets.Add(stand.Key, damageGroup);
                    }
                }
            }
            // 搜索符合条件的虚拟单位
            foreach (TechnoExt vuint in TechnoStatusScript.VirtualUnitArray)
            {
                Pointer<TechnoClass> pTarget = vuint.OwnerObject;
                if (!targets.ContainsKey(vuint) && pTarget.Convert<ObjectClass>() != exclude
                    && CheckAndMarkTarget(pTarget, spread, location, damage, pAttacker, pWH, pAttackingHouse, out DamageGroup damageGroup))
                {
                    targets.Add(vuint, damageGroup);
                }
            }

            // Logger.Log($"{Game.CurrentFrame} 弹头[{pWH.Ref.Base.ID}] {pWH} 爆炸半径{pWH.Ref.CellSpread}, 影响的替身或虚拟单位有{targets.Count()}个，造成伤害 {damage}");
            foreach (DamageGroup damageGroup in targets.Values)
            {
                // Logger.Log($"{Game.CurrentFrame} 弹头[{pWH.Ref.Base.ID}] {pWH} 爆炸半径{pWH.Ref.CellSpread}, 炸掉目标 [{damageGroup.Target.Ref.Type.Ref.Base.Base.ID}]{damageGroup.Target}");
                damageGroup.Target.Ref.Base.ReceiveDamage(damage, (int)damageGroup.Distance, pWH, pAttacker, false, false, pAttackingHouse);
            }
        }

        private static bool CheckAndMarkTarget(Pointer<TechnoClass> pTarget, double spread, CoordStruct location, int damage, Pointer<ObjectClass> pAttacker,
           Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pAttackingHouse, out DamageGroup damageGroup)
        {
            damageGroup = default;
            if (!pTarget.IsNull && !pTarget.Ref.Type.IsNull && !pTarget.IsImmune())
            {
                // 检查距离
                CoordStruct targetPos = pTarget.Ref.Base.Base.GetCoords();
                double dist = targetPos.DistanceFrom(location);
                // Logger.Log($"{Game.CurrentFrame} 检查目标 [{pTarget.Ref.Type.Ref.Base.Base.ID}]{pTarget} 与目标点的距离 {dist}");
                if (pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft && pTarget.InAir())
                {
                    dist *= 0.5;
                }
                if (dist <= spread)
                {
                    // 找到一个在范围内的目标，检查弹头是否可以影响该目标
                    if (pTarget.CanAffectMe(pAttackingHouse, pWH)// 检查所属权限
                        && pTarget.CanDamageMe(damage, (int)dist, pWH, out int realDamage)// 检查护甲
                    )
                    {
                        damageGroup = new DamageGroup();
                        damageGroup.Target = pTarget;
                        damageGroup.Distance = dist;
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
