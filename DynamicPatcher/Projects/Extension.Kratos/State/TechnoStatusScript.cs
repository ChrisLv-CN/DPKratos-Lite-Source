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
    [UpdateBefore(typeof(AttachEffectScript))]
    public partial class TechnoStatusScript : TechnoScriptable
    {
        public TechnoStatusScript(TechnoExt owner) : base(owner) { }

        public bool DisableVoxelCache;
        public float VoxelShadowScaleInAir;

        public bool DisableSelectVoice;

        public bool IsBuilding;

        public DrivingState DrivingState;
        private Mission lastMission;

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
            // 染色弹
            heir.PaintballState = this.PaintballState;
            this.PaintballState = new PaintballState();
        }

        public override void Awake()
        {
            this.VoxelShadowScaleInAir = Ini.GetSection(Ini.RulesDependency, RulesClass.SectionAudioVisual).Get("VoxelShadowScaleInAir", 2f);
            this.IsBuilding = pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Building;
        }

        public override void OnPut(Pointer<CoordStruct> coord, DirType dirType)
        {
            OnPut_AttackBeacon();
            OnPut_BlackHole();
            OnPut_Deselect();
            OnPut_DestroySelf();
            OnPut_ExtraFire();
            OnPut_FireSuper();
            OnPut_GiftBox();
            OnPut_OverrideWeapon();
            OnPut_Paintball();
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
                OnUpdate_Deselect();
                OnUpdate_GiftBox();
                OnUpdate_Paintball();
            }
        }

        public override void OnLateUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                this.lastMission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
            }
        }

        public override void OnReceiveDamage(Pointer<int> pDamage, int distanceFromEpicenter, Pointer<WarheadTypeClass> pWH, Pointer<ObjectClass> pAttacker, bool ignoreDefenses, bool preventPassengerEscape, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pTechno.IsDeadOrInvisible() && !pTechno.Ref.Target.IsNull)
            {
                WarheadTypeData warheadTypeData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
                if (warheadTypeData.ClearTarget)
                {
                    ClearTarget();
                }

                OnReceiveDamage_Stand(pDamage, distanceFromEpicenter, pWH, pAttacker, ignoreDefenses, preventPassengerEscape, pAttackingHouse);
            }
        }

        public override void OnReceiveDamage2(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            OnReceiveDamage2_GiftBox(pRealDamage, pWH, damageState, pAttacker, pAttackingHouse);
        }

        public override void OnReceiveDamageDestroy()
        {
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
    }
}