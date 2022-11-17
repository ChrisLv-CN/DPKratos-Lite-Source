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
    [GlobalScriptable(typeof(TechnoExt))]
    // [UpdateAfter(typeof(AttachEffectScript))]
    public partial class TechnoStatusScript : TechnoScriptable
    {
        public TechnoStatusScript(TechnoExt owner) : base(owner) { }

        public bool DisableVoxelCache;
        public float VoxelShadowScaleInAir;

        public bool DisableSelectVoice;


        public bool CantMove;

        public DrivingState DrivingState;
        private Mission lastMission;

        private bool isBuilding;

        private bool initStateFlag = false;

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
            this.ExtraFireState = new State<ExtraFireData>();
            // 发射超武
            heir.FireSuperState = this.FireSuperState;
            this.FireSuperState = new State<FireSuperData>();
            // 覆盖武器
            heir.OverrideWeaponState = this.OverrideWeaponState;
            this.OverrideWeaponState = new OverrideWeaponState();
            // 传送
            heir.TeleportState = this.TeleportState;
            this.TeleportState = new TeleportState();
            // 染色弹
            heir.PaintballState = this.PaintballState;
            this.PaintballState = new PaintballState();
        }

        public override void Awake()
        {
            this.VoxelShadowScaleInAir = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual).Get("VoxelShadowScaleInAir", 2f);
            this.isBuilding = pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building;
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, ref DirType dirType)
        {
            OnPut_StandUnit(pCoord, dirType);

            if (!initStateFlag)
            {
                initStateFlag = true;
                InitState_AttackBeacon();
                InitState_BlackHole();
                InitState_DamageReaction();
                InitState_Deselect();
                InitState_DestroySelf();
                InitState_ExtraFire();
                InitState_FireSuper();
                InitState_GiftBox();
                InitState_OverrideWeapon();
                InitState_Paintball();
                InitState_Teleport();
                InitState_VirtualUnit();
            }
        }

        public override void OnUpdate()
        {
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
                OnUpdate_AttackBeacon();
                OnUpdate_BlackHole();
                OnUpdate_DamageReaction();
                OnUpdate_Deselect();
                OnUpdate_GiftBox();
                OnUpdate_Paintball();
                OnUpdate_Teleport();
            }
        }

        public override void OnLateUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                this.lastMission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
            }
        }

        public override void OnRemove()
        {
            OnRemove_StandUnit();
        }

        public override void OnUnInit()
        {
            OnRemove_StandUnit();
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                if (!pTechno.Ref.Target.IsNull)
                {
                    WarheadTypeData warheadTypeData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                    if (warheadTypeData.ClearTarget)
                    {
                        ClearTarget();
                    }
                }
                OnReceiveDamage_DamageReaction(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
                OnReceiveDamage_Stand(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            }
        }

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            OnReceiveDamage2_BlackHole(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
            OnReceiveDamage2_GiftBox(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamageDestroy()
        {
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
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            OnFire_AttackBeacon_Recruit(pTarget, weaponIndex);
            OnFire_ExtraFire(pTarget, weaponIndex);
            OnFire_FireSuper(pTarget, weaponIndex);
            OnFire_OverrideWeapon(pTarget, weaponIndex);
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

        public void ClearTarget()
        {
            pTechno.Ref.Target = IntPtr.Zero;
            pTechno.Ref.SetTarget(IntPtr.Zero);
            // OwnerObject.Convert<MissionClass>().Ref.QueueMission(Mission.Stop, true);
            if (!pTechno.Ref.SpawnManager.IsNull)
            {
                pTechno.Ref.SpawnManager.Ref.Destination = IntPtr.Zero;
                pTechno.Ref.SpawnManager.Ref.Target = IntPtr.Zero;
                pTechno.Ref.SpawnManager.Ref.SetTarget(IntPtr.Zero);
            }
        }

        public void StopMoving()
        {
            Pointer<FootClass> pFoot = pTechno.Convert<FootClass>();
            // LocomotionClass.ChangeLocomotorTo(pFoot, LocomotionClass.Jumpjet);
            ILocomotion loco = pFoot.Ref.Locomotor;
            loco.Mark_All_Occupation_Bits((int)MarkType.UP); // 清除HeadTo的占领
            if (loco.Apparent_Speed() > 0)
            {
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响 speed={loco.Apparent_Speed()} IsMoving={loco.Is_Moving()} IsMovingNow={loco.Is_Moving_Now()} IsReallyMovingNow={loco.Is_Really_Moving_Now()}");
                pFoot.Ref.Base.SetDestination(default(Pointer<CellClass>));
                loco.ForceStopMoving();
            }
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响 speed={loco.Apparent_Speed()} IsMoving={loco.Is_Moving()} IsMovingNow={loco.Is_Moving_Now()} IsReallyMovingNow={loco.Is_Really_Moving_Now()}");
            loco.Lock();
            // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 停止行动");)
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
