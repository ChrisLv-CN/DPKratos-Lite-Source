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

    public partial class AttachEffect
    {

        public Stand Stand;

        private void InitStand()
        {
            this.Stand = AEData.StandData.CreateEffect<Stand>();
            RegisterEffect(Stand);
        }
    }


    [Serializable]
    public class Stand : Effect<StandData>
    {
        public TechnoExt StandExt;
        public Pointer<TechnoClass> pStand => null != StandExt ? StandExt.OwnerObject : default;
        public CoordStruct Offset => Data.Offset;

        private Pointer<ObjectClass> pMaster => AE.pOwner;

        private bool masterIsBullet;

        private bool masterIsRocket = false;
        private bool masterIsSpawned = false;
        private bool standIsBuilding = false;
        private bool onStopCommand = false;
        private bool notBeHuman = false;
        private bool onReceiveDamageDestroy = false;

        private LocationMark lastLocationMark;
        private bool isMoving = false;
        private TimerStruct waklRateTimer;

        public override bool IsAlive()
        {
            // 下沉和坠毁不算死亡
            if (pStand.Convert<ObjectClass>().IsDead())
            {
                return false;
            }
            return true;
        }

        // 激活
        public override void OnEnable()
        {
            this.masterIsBullet = AE.AEManager.IsBullet;
            CreateAndPutStand();
        }

        private void CreateAndPutStand()
        {
            CoordStruct location = pMaster.Ref.Base.GetCoords();

            Pointer<TechnoTypeClass> pType = TechnoTypeClass.Find(Data.Type);
            if (!pType.IsNull)
            {
                // 创建替身
                Pointer<TechnoClass> pNewTechno = pType.Ref.Base.CreateObject(AE.pSourceHouse).Convert<TechnoClass>();
                this.StandExt = TechnoExt.ExtMap.Find(pNewTechno);
                if (!pStand.IsNull)
                {
                    // 同步部分扩展设置
                    if (pStand.TryGetStatus(out TechnoStatusScript standStatus))
                    {
                        standStatus.VirtualUnit = this.Data.VirtualUnit;
                        standStatus.StandData = this.Data;

                        // 设置替身的所有者
                        if (pMaster.CastToTechno(out Pointer<TechnoClass> pTechno))
                        {
                            standStatus.MyMasterExt = TechnoExt.ExtMap.Find(pTechno);
                            // 必须是同一阵营
                            if (!pTechno.Ref.Owner.IsNull && pTechno.Ref.Owner == AE.pSourceHouse && pTechno.TryGetStatus(out TechnoStatusScript masterStatus))
                            {
                                // 同步AE状态机
                                // 染色
                                standStatus.PaintballState = masterStatus.PaintballState;
                                // 乘客
                                // if (Data.SamePassengers)
                                // {
                                //     Logger.Log($"{Game.CurrentFrame} 同步替身乘客空间");
                                //     pStand.Ref.Passengers = pTechno.Ref.Passengers;
                                // }
                            }
                            if (pTechno.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                            {
                                masterIsRocket = pTechno.Ref.Type.Ref.MissileSpawn;
                                masterIsSpawned = masterIsRocket || pTechno.Ref.Type.Ref.Spawned;
                                standStatus.MyMasterIsSpawned = masterIsSpawned;
                            }
                        }
                        else if (pMaster.CastToBullet(out Pointer<BulletClass> pBullet))
                        {
                            // 附加在抛射体上的，取抛射体的所有者
                            if (!pBullet.Ref.Owner.IsNull)
                            {
                                standStatus.MyMasterExt = TechnoExt.ExtMap.Find(pBullet.Ref.Owner);
                            }
                        }
                    }
                    // 初始化替身
                    pStand.Ref.Base.Mark(MarkType.UP); // 拔起，不在地图上
                    bool canGuard = AE.pSourceHouse.Ref.ControlledByHuman();
                    if (pStand.Ref.Base.Base.WhatAmI() == AbstractType.Building)
                    {
                        standIsBuilding = true;
                        canGuard = true;
                    }
                    else
                    {
                        // lock locomotor
                        pStand.Convert<FootClass>().Ref.Locomotor.Lock();
                    }
                    // only computer units can hunt
                    Mission mission = canGuard ? Mission.Guard : Mission.Hunt;
                    pStand.Convert<MissionClass>().Ref.QueueMission(mission, false);

                    if (!pMaster.IsInvisible())
                    {
                        // Logger.Log($"{Game.CurrentFrame} - put stand [{Data.Type}]{pStand} on {location}");
                        // 在格子位置刷出替身单位
                        if (!TryPutStand(location))
                        {
                            // 刷不出来？
                            Disable(location);
                            return;
                        }
                    }

                    // 放置到指定位置
                    LocationMark locationMark = pMaster.GetRelativeLocation(Offset, Data.Direction, Data.IsOnTurret, Data.IsOnWorld);
                    if (default != locationMark.Location)
                    {
                        SetLocation(locationMark.Location);
                        // 强扭朝向
                        ForceSetFacing(locationMark.Direction);
                    }
                    // Logger.Log($"{Game.CurrentFrame} - 创建替身 [{Data.Type}]{pStand}, 所属 {AE.pSourceHouse}, JOJO [{pOwner.Ref.Type.Ref.Base.ID}]{pOwner}");

                    Pointer<TechnoClass> pSource = AE.pSource;
                    if (Data.AttackSource && !pSource.IsDeadOrInvisible() && pStand.CanAttack(pSource))
                    {
                        // Logger.Log($"{Game.CurrentFrame} 替身[{Data.Type}]{pStand}强制设置来源[{pSource.Ref.Type.Ref.Base.Base.ID}]{pSource}为目标");
                        pStand.Ref.SetTarget(pSource.Convert<AbstractClass>());
                    }
                }
            }

        }

        private bool TryPutStand(CoordStruct location)
        {
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {
                var occFlags = pCell.Ref.OccupationFlags;
                // if (occFlags.HasFlag(OccupationFlags.Buildings))
                // {
                //     // Logger.Log("当前格子上有建筑");
                //     BulletEffectHelper.RedCell(pCell.Ref.GetCoordsWithBridge(), 128, 1, 450);
                //     BulletEffectHelper.RedLineZ(pCell.Ref.GetCoordsWithBridge(), 1024, 1, 450);
                //     SpeedType speedType = pStand.Ref.Type.Ref.SpeedType;
                //     MovementZone movementZone = pStand.Ref.Type.Ref.MovementZone;
                //     CellStruct cell = MapClass.Instance.Pathfinding_Find(ref pCell.Ref.MapCoords, speedType, movementZone, 1, 1, false);
                //     if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pNewCell))
                //     {
                //         pCell = pNewCell;
                //         occFlags = pCell.Ref.OccupationFlags;
                //         Pointer<ObjectClass> pObject = pCell.Ref.GetContent();
                //         // Logger.Log("找到一个满足条件的空格子, occFlags = {0}, pObject = {1}", occFlags, pObject.IsNull ? "null" : pObject.Ref.Base.WhatAmI());
                //     }
                // }
                pStand.Ref.Base.OnBridge = pCell.Ref.ContainsBridge();
                CoordStruct xyz = pCell.Ref.GetCoordsWithBridge();
                ++Game.IKnowWhatImDoing;
                pStand.Ref.Base.Put(xyz, 0);
                --Game.IKnowWhatImDoing;
                pCell.Ref.OccupationFlags = occFlags;
                return true;
            }
            // ++Game.IKnowWhatImDoing;
            // bool isPut = pStand.Ref.Base.Put(location, 0);
            // --Game.IKnowWhatImDoing;
            return false;
        }

        // 销毁
        public override void OnDisable(CoordStruct location)
        {
            // Logger.Log($"{Game.CurrentFrame} - {AE.AEData.Name} 替身 {Data.Type} 销毁");
            if (pStand.IsNull)
            {
                return;
            }
            ExplodesOrDisappear(false);
        }

        private void ExplodesOrDisappear(bool peaceful)
        {
            // Logger.Log($"{Game.CurrentFrame} {AE.AEData.Name} 替身 [{Data.Type}]{pStand} 注销");
            bool explodes = !peaceful && (Data.Explodes || notBeHuman || (masterIsRocket && Data.ExplodesWithRocket)) && !pStand.Ref.BeingWarpedOut && !pStand.Ref.WarpingOut;
            if (pStand.TryGetStatus(out TechnoStatusScript standStatus))
            {
                // Logger.Log($"{Game.CurrentFrame} 阿伟 [{Data.Type}]{pStand} 要死了 explodes = {explodes}");
                standStatus.DestroySelfState.DestroyNow(!explodes);
                if (masterIsRocket)
                {
                    standStatus.OnUpdate_DestroySelf();
                }
            }
            else
            {
                if (explodes)
                {
                    // Logger.Log($"{Game.CurrentFrame} {AEType.Name} 替身 {pStand}[{Type.Type}] 自爆, 没有发现EXT");
                    pStand.Ref.Base.TakeDamage(pStand.Ref.Base.Health + 1, pStand.Ref.Type.Ref.Crewed);
                }
                else
                {
                    // Logger.Log($"{Game.CurrentFrame} {AEType.Name} 替身 {Type.Type} 移除, 没有发现EXT");
                    pStand.Ref.Base.Remove();
                    // pStand.Ref.Base.UnInit(); // 替身攻击建筑时死亡会导致崩溃，莫名其妙的bug
                    pStand.Ref.Base.TakeDamage(pStand.Ref.Base.Health + 1, false);
                }
            }
            StandExt = null;
        }

        public override void OnGScreenRender(CoordStruct location)
        {
            if (!standIsBuilding && pMaster.CastToFoot(out Pointer<FootClass> pMasterFoot))
            {
                // synch Tilt
                if (!Data.IsTrain)
                {
                    // rocker Squid capture ship
                    // pStand.Ref.AngleRotatedForwards = pMaster.Ref.Base.AngleRotatedForwards;
                    // pStand.Ref.AngleRotatedSideways = pMaster.Ref.Base.AngleRotatedSideways;

                    if (Data.SameTilter)
                    {
                        float forwards = pMasterFoot.Ref.Base.AngleRotatedForwards;
                        float sideways = pMasterFoot.Ref.Base.AngleRotatedSideways;
                        float t = 0f;
                        // Logger.Log($"{Game.CurrentFrame} 替身 朝向 {Type.Direction}, forwards = {forwards}, sideways = {sideways}");
                        // 计算方向
                        switch (Data.Direction)
                        {
                            case 0: // 正前 N
                                break;
                            case 2: // 前右 NE
                                break;
                            case 4: // 正右 E
                                t = forwards;
                                forwards = -sideways;
                                sideways = t;
                                break;
                            case 6: // 右后 SE
                                break;
                            case 8: // 正后 S
                                sideways = -sideways;
                                break;
                            case 10: // 后左 SW
                            case 12: // 正左 W
                                t = forwards;
                                forwards = sideways;
                                sideways = -t;
                                break;
                            case 14: // 前左 NW
                                break;
                        }
                        pStand.Ref.AngleRotatedForwards = forwards;
                        pStand.Ref.AngleRotatedSideways = sideways;
                        pStand.Ref.RockingForwardsPerFrame = forwards;
                        pStand.Ref.RockingSidewaysPerFrame = sideways;

                        // Logger.Log($"{Game.CurrentFrame} 同步 替身 与 JOJO 的翻车角度");

                        ILocomotion masterLoco = pMasterFoot.Ref.Locomotor;
                        ILocomotion standLoco = pStand.Convert<FootClass>().Ref.Locomotor;

                        Guid masterLocoId = masterLoco.ToLocomotionClass().Ref.GetClassID();
                        Guid standLocoId = standLoco.ToLocomotionClass().Ref.GetClassID();
                        // Logger.Log($"{Game.CurrentFrame} 替身的 LocoID = {standLocoId}, JOJO的 LocoID = {masterLocoId}");
                        if (masterLocoId == LocomotionClass.Drive && standLocoId == LocomotionClass.Drive)
                        {
                            Pointer<DriveLocomotionClass> pMasterLoco = masterLoco.ToLocomotionClass<DriveLocomotionClass>();
                            Pointer<DriveLocomotionClass> pStandLoco = masterLoco.ToLocomotionClass<DriveLocomotionClass>();
                            // Logger.Log($"{Game.CurrentFrame} 同步替身 {pStand} 的 车 倾斜度与JOJO {pMaster} 相同, Stand.Ramp1 = {pStandLoco.Ref.Ramp1}, Stand.Ramp2 = {pStandLoco.Ref.Ramp2}");
                            // Logger.Log($"{Game.CurrentFrame} 同步替身 {pStand} 的 车 倾斜度与JOJO {pMaster} 相同, Master.Ramp1 = {pMasterLoco.Ref.Ramp1}, Master.Ramp2 = {pMasterLoco.Ref.Ramp2}");
                            pStandLoco.Ref.Ramp1 = pMasterLoco.Ref.Ramp1;
                            pStandLoco.Ref.Ramp2 = pMasterLoco.Ref.Ramp2;
                        }
                        else if (masterLocoId == LocomotionClass.Ship && standLocoId == LocomotionClass.Ship)
                        {
                            Pointer<ShipLocomotionClass> pMasterLoco = masterLoco.ToLocomotionClass<ShipLocomotionClass>();
                            Pointer<ShipLocomotionClass> pStandLoco = masterLoco.ToLocomotionClass<ShipLocomotionClass>();
                            // Logger.Log($"{Game.CurrentFrame} 同步替身 {pStand} 的 船 倾斜度与JOJO {pMaster} 相同, Stand.Ramp1 = {pStandLoco.Ref.Ramp1}, Stand.Ramp2 = {pStandLoco.Ref.Ramp2}");
                            // Logger.Log($"{Game.CurrentFrame} 同步替身 {pStand} 的 船 倾斜度与JOJO {pMaster} 相同, Master.Ramp1 = {pMasterLoco.Ref.Ramp1}, Master.Ramp2 = {pMasterLoco.Ref.Ramp2}");
                            pStandLoco.Ref.Ramp1 = pMasterLoco.Ref.Ramp1;
                            pStandLoco.Ref.Ramp2 = pMasterLoco.Ref.Ramp2;
                        }
                    }
                }
            }
        }

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            // 只同步状态，位置和朝向由StandManager控制
            if (masterIsBullet)
            {
                UpdateState(pMaster.Convert<BulletClass>());
            }
            else
            {
                UpdateState(pMaster.Convert<TechnoClass>(), isDead);
            }
        }

        public override void OnWarpUpdate(CoordStruct location, bool isDead)
        {
            // 只同步状态，位置和朝向由StandManager控制
            if (masterIsBullet)
            {
                UpdateState(pMaster.Convert<BulletClass>());
            }
            else
            {
                UpdateState(pMaster.Convert<TechnoClass>(), isDead);
            }
        }

        // public override void OnTemporalUpdate(Pointer<TemporalClass> pTemporal)
        // {
        //     if (pMaster.CastToTechno(out Pointer<TechnoClass> pMasterTechno))
        //     {
        //         // 除去攻击jojo的超时空军团
        //         if (pStand.Ref.Owner == pMasterTechno.Ref.Owner && !(Data.ForceAttackMaster && !pStand.Ref.TemporalImUsing.IsNull))
        //         {
        //             pStand.Ref.BeingWarpedOut = pMasterTechno.Ref.BeingWarpedOut; // 被超时空兵冻结
        //         }
        //     }
        // }

        public override void OnTemporalEliminate(Pointer<TemporalClass> pTemporal)
        {
            // Logger.Log($"{Game.CurrentFrame} 替身 [{Data.Type}]{pStand} 的使者 [{pMaster.Ref.Type.Ref.Base.ID}]{pMaster} 被超时空抹除");
            Disable(pMaster.Ref.Base.GetCoords());
        }

        public override void OnRocketExplosion()
        {
            // Logger.Log($"{Game.CurrentFrame} 替身 [{Data.Type}]{pStand} 的使者 [{pMaster.Ref.Type.Ref.Base.ID}]{pMaster} 是导弹，自爆了");
            if (!onReceiveDamageDestroy)
            {
                ExplodesOrDisappear(Data.ExplodesWithMaster);
            }
        }

        public void UpdateState(Pointer<BulletClass> pBullet)
        {
            if (pStand.IsNull)
            {
                return;
            }
            // Logger.Log($"{Game.CurrentFrame} 抛射体上的 {AEType.Name} 替身 {Type.Type} {(pStand.Ref.Base.IsAlive ? "存活" : "死亡")}");
            // Synch Target
            RemoveStandIllegalTarget();
            Pointer<AbstractClass> target = pBullet.Ref.Target;
            if (Data.SameTarget && !target.IsNull)
            {
                pStand.Ref.SetTarget(target);
            }
            if (Data.SameLoseTarget && target.IsNull)
            {
                pStand.Ref.SetTarget(target);
                if (target.IsNull && !pStand.Ref.SpawnManager.IsNull)
                {
                    pStand.Ref.SpawnManager.Ref.Destination = target;
                    pStand.Ref.SetTarget(target);
                }
            }
            switch (Data.Targeting)
            {
                case StandTargeting.LAND:
                    if (pStand.InAir())
                    {
                        pStand.ClearAllTarget();
                    }
                    break;
                case StandTargeting.AIR:
                    if (!pStand.InAir())
                    {
                        pStand.ClearAllTarget();
                    }
                    break;
            }
        }

        public void UpdateState(Pointer<TechnoClass> pMaster, bool masterIsDead)
        {
            if (pStand.IsNull || pMaster.IsNull)
            {
                return;
            }
            // Logger.Log($"{Game.CurrentFrame} 单位上的 {AE.AEData.Name} 替身 {Data.Type} {(pStand.Ref.Base.IsAlive ? "存活" : "死亡")}");
            if (pMaster.Ref.IsSinking && Data.RemoveAtSinking)
            {
                // Logger.Log($"{Game.CurrentFrame} 船沉了，自爆吧！");
                ExplodesOrDisappear(true);
                return;
            }
            if (masterIsDead && AE.AEManager.InSelling)
            {
                // Logger.Log($"{Game.CurrentFrame} 建筑卖了，消失吧！");
                ExplodesOrDisappear(true);
                return;
            }
            // reset state
            pStand.Ref.Base.Mark(MarkType.UP); // 拔起，不在地图上
            // pStand.Ref.Base.IsOnMap = false;
            // pStand.Ref.Base.NeedsRedraw = true;

            if (Data.SameHouse)
            {
                // synch Owner
                pStand.Ref.SetOwningHouse(pMaster.Ref.Owner);
            }


            // synch State
            pStand.Ref.IsSinking = pMaster.Ref.IsSinking;
            pStand.Ref.Shipsink_3CA = pMaster.Ref.Shipsink_3CA;
            pStand.Ref.Base.InLimbo = pMaster.Ref.Base.InLimbo;
            pStand.Ref.Base.OnBridge = pMaster.Ref.Base.OnBridge;
            if (pMaster.Ref.Owner == pStand.Ref.Owner)
            {
                // 同阵营限定
                pStand.Ref.Cloakable = pMaster.Ref.Cloakable;
                pStand.Ref.CloakStates = pMaster.Ref.CloakStates;
                pStand.Ref.WarpingOut = pMaster.Ref.WarpingOut; // 超时空传送冻结
                // 替身不是强制攻击jojo的超时空兵替身
                if (!Data.ForceAttackMaster || pStand.Ref.TemporalImUsing.IsNull)
                {
                    pStand.Ref.BeingWarpedOut = pMaster.Ref.BeingWarpedOut; // 被超时空兵冻结
                }
                pStand.Ref.Deactivated = pMaster.Ref.Deactivated; // 遥控坦克

                pStand.Ref.IronCurtainTimer = pMaster.Ref.IronCurtainTimer;
                pStand.Ref.IronTintTimer = pMaster.Ref.IronTintTimer;
                // pStand.Ref.CloakDelayTimer = pMaster.Ref.CloakDelayTimer; // 反复进入隐形
                pStand.Ref.IdleActionTimer = pMaster.Ref.IdleActionTimer;
                pStand.Ref.Berzerk = pMaster.Ref.Berzerk;
                pStand.Ref.EMPLockRemaining = pMaster.Ref.EMPLockRemaining;
                pStand.Ref.ShouldLoseTargetNow = pMaster.Ref.ShouldLoseTargetNow;

                // synch status
                if (Data.IsVirtualTurret)
                {
                    pStand.Ref.FirepowerMultiplier = pMaster.Ref.FirepowerMultiplier;
                    pStand.Ref.ArmorMultiplier = pMaster.Ref.ArmorMultiplier;
                }

                // synch ammo
                if (Data.SameAmmo)
                {
                    pStand.Ref.Ammo = pMaster.Ref.Ammo;
                }

                // synch Passengers
                if (Data.SamePassengers)
                {
                    // Pointer<FootClass> pPassenger = pMaster.Ref.Passengers.FirstPassenger;
                    // if (!pPassenger.IsNull)
                    // {
                    //     Pointer<TechnoTypeClass> pType = pPassenger.Ref.Base.Type;
                    //     Pointer<TechnoClass> pNew = pType.Ref.Base.CreateObject(AE.pSourceHouse).Convert<TechnoClass>();
                    //     pNew.Ref.Base.Put(default, DirType.N);
                    //     Logger.Log($"{Game.CurrentFrame} 把jojo的乘客塞进替身里");
                    //     pStand.Ref.Passengers.AddPassenger(pNew.Convert<FootClass>());
                    // }
                }

                // synch Promote
                if (pStand.Ref.Type.Ref.Trainable)
                {
                    if (Data.PromoteFromSpawnOwner && masterIsSpawned && !pMaster.Ref.SpawnOwner.IsDead())
                    {
                        pStand.Ref.Veterancy = pMaster.Ref.SpawnOwner.Ref.Veterancy;
                    }
                    else if (Data.PromoteFromMaster)
                    {
                        pStand.Ref.Veterancy = pMaster.Ref.Veterancy;
                    }
                }

                // synch PrimaryFactory
                pStand.Ref.IsPrimaryFactory = pMaster.Ref.IsPrimaryFactory;
            }

            if (pStand.IsInvisible())
            {
                pStand.ClearAllTarget();
                return;
            }

            // get mission
            Mission masterMission = pMaster.Convert<MissionClass>().Ref.CurrentMission;

            // check power off and moving
            bool masterIsBuilding = false;
            bool masterPowerOff = pMaster.Ref.Owner.Ref.NoPower;
            bool masterIsMoving = masterMission == Mission.Move || masterMission == Mission.AttackMove;
            if (masterIsBuilding = (pMaster.Ref.Base.Base.WhatAmI() == AbstractType.Building))
            {
                if (pMaster.Ref.Owner == pStand.Ref.Owner)
                {
                    // synch focus
                    if (standIsBuilding)
                    {
                        pStand.Ref.Focus = pMaster.Ref.Focus;
                    }
                    // check poweroff
                    Pointer<BuildingClass> pBuilding = pMaster.Convert<BuildingClass>();
                    if (!masterPowerOff)
                    {
                        // 关闭当前建筑电源
                        masterPowerOff = !pBuilding.Ref.HasPower;
                    }
                }
            }
            else if (!masterIsMoving)
            {
                Pointer<FootClass> pFoot = pMaster.Convert<FootClass>();
                masterIsMoving = pFoot.Ref.Locomotor.Is_Moving() && pFoot.Ref.GetCurrentSpeed() > 0;
            }

            // check fire
            bool powerOff = Data.Powered && masterPowerOff;
            bool canFire = !powerOff && (Data.MobileFire || !masterIsMoving);
            if (canFire)
            {
                // synch mission
                switch (masterMission)
                {
                    case Mission.Guard:
                    case Mission.Area_Guard:
                        Mission standMission = pStand.Convert<MissionClass>().Ref.CurrentMission;
                        if (standMission != Mission.Attack)
                        {
                            pStand.Convert<MissionClass>().Ref.QueueMission(masterMission, true);
                        }
                        break;
                }
            }
            else
            {
                pStand.ClearAllTarget();
                onStopCommand = false;
                pStand.Convert<MissionClass>().Ref.QueueMission(Mission.Sleep, true);
            }

            // synch target
            if (Data.ForceAttackMaster)
            {
                if (!powerOff && !masterIsDead)
                {
                    Pointer<AbstractClass> pTarget = pMaster.Convert<AbstractClass>();
                    // 替身是超时空兵，被冻住时不能开火，需要特殊处理
                    if (pStand.Ref.BeingWarpedOut && !pStand.Ref.TemporalImUsing.IsNull)
                    {
                        pStand.Ref.BeingWarpedOut = false;
                        if (pStand.CanAttack(pTarget))
                        {
                            // 检查ROF
                            if (pStand.Ref.ROFTimer.Expired())
                            {
                                int weaponIdx = pStand.Ref.SelectWeapon(pTarget);
                                // Logger.Log($"{Game.CurrentFrame} [{Data.Type}]{pStand} 向 JOJO {pTarget} 发射武器");
                                pStand.Ref.Fire_IgnoreType(pTarget, weaponIdx);
                                int rof = 0;
                                Pointer<WeaponStruct> pWeapon = pStand.Ref.GetWeapon(weaponIdx);
                                if (!pWeapon.IsNull && !pWeapon.Ref.WeaponType.IsNull)
                                {
                                    rof = pWeapon.Ref.WeaponType.Ref.ROF;
                                }
                                if (rof > 0)
                                {
                                    pStand.Ref.ROFTimer.Start(rof);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (pStand.CanAttack(pTarget))
                        {
                            pStand.Ref.SetTarget(pTarget);
                        }
                    }
                }
            }
            else
            {
                if (!onStopCommand)
                {
                    // synch Target
                    RemoveStandIllegalTarget();
                    Pointer<AbstractClass> target = pMaster.Ref.Target;
                    if (!target.IsNull)
                    {
                        if (Data.SameTarget && canFire && pStand.CanAttack(target))
                        {
                            pStand.Ref.SetTarget(target);
                        }
                    }
                    else
                    {
                        if (Data.SameLoseTarget || !canFire)
                        {
                            pStand.ClearAllTarget();
                        }
                    }
                }
                else
                {
                    onStopCommand = false;
                }
            }
            switch (Data.Targeting)
            {
                case StandTargeting.LAND:
                    if (pStand.InAir())
                    {
                        pStand.ClearAllTarget();
                    }
                    break;
                case StandTargeting.AIR:
                    if (!pStand.InAir())
                    {
                        pStand.ClearAllTarget();
                    }
                    break;
            }

            // synch Moving anim
            if (Data.IsTrain || Data.SameMoving)
            {
                Pointer<FootClass> pFoot = pStand.Convert<FootClass>();
                ILocomotion loco = pFoot.Ref.Locomotor;
                Guid locoId = loco.ToLocomotionClass().Ref.GetClassID();
                if (locoId == LocomotionClass.Drive || locoId == LocomotionClass.Walk || locoId == LocomotionClass.Mech)
                {
                    if (masterIsMoving)
                    {
                        if (isMoving)
                        {
                            if (!Data.IsTrain)
                            {
                                // 移动前，设置替身的朝向与JOJO相同
                                pStand.Ref.Facing.set(pMaster.Ref.Facing.current());
                            }
                            // 往前移动，播放移动动画
                            if (waklRateTimer.Expired())
                            {
                                // VXL只需要帧动起来，就会播放动画
                                // 但SHP动画，还需要检查Loco.Is_Moving()为true时，才可以播放动画 0x73C69D
                                pFoot.Ref.WalkedFramesSoFar_idle++;
                                waklRateTimer.Start(pFoot.Ref.Base.Type.Ref.WalkRate);
                            }
                            // 为SHP素材设置一个总的运动标记
                            if (pStand.TryGetStatus(out TechnoStatusScript status))
                            {
                                status.StandIsMoving = true;
                            }
                            // DriveLoco.Is_Moving()并不会判断IsDriving
                            // ShipLoco.Is_Moving()并不会判断IsDriving
                            // HoverLoco.Is_Moving()与前面两个一样，只用位置判断是否在运动
                            // 以上几个是通过判断位置来确定是否在运动
                            // WalkLoco和MechLoco则只返回IsMoving来判断是否在运动
                            if (locoId == LocomotionClass.Walk)
                            {
                                Pointer<WalkLocomotionClass> pLoco = loco.ToLocomotionClass<WalkLocomotionClass>();
                                pLoco.Ref.IsReallyMoving = true;
                            }
                            else if (locoId == LocomotionClass.Mech)
                            {
                                Pointer<MechLocomotionClass> pLoco = loco.ToLocomotionClass<MechLocomotionClass>();
                                pLoco.Ref.IsMoving = true;
                            }
                        }
                    }
                    else
                    {
                        if (isMoving)
                        {
                            // 停止移动
                            // 为SHP素材设置一个总的运动标记
                            if (pStand.TryGetStatus(out TechnoStatusScript status))
                            {
                                status.StandIsMoving = false;
                            }
                            // loco.ForceStopMoving();
                            if (locoId == LocomotionClass.Walk)
                            {
                                Pointer<WalkLocomotionClass> pLoco = loco.ToLocomotionClass<WalkLocomotionClass>();
                                pLoco.Ref.IsReallyMoving = false;
                            }
                            else if (locoId == LocomotionClass.Mech)
                            {
                                Pointer<MechLocomotionClass> pLoco = loco.ToLocomotionClass<MechLocomotionClass>();
                                pLoco.Ref.IsMoving = false;
                            }
                        }
                        isMoving = false;
                    }
                }
            }
        }

        // private bool StandCanAttackTarget(Pointer<AbstractClass> pTarget, bool checkRange)
        // {
        //     int i = pStand.Ref.SelectWeapon(pTarget);
        //     FireError fireError = pStand.Ref.GetFireError(pTarget, i, checkRange);
        //     // Logger.Log($"{Game.CurrentFrame} [{Data.Type}]{pStand} checkRange = {checkRange}, {fireError}, WarpingOut = {pStand.Ref.WarpingOut}, BeingWarpedOut = {pStand.Ref.BeingWarpedOut}");
        //     switch (fireError)
        //     {
        //         case FireError.ILLEGAL:
        //         case FireError.CANT:
        //         case FireError.MOVING:
        //         case FireError.RANGE:
        //             return false;
        //     }
        //     return true;
        // }

        private void RemoveStandIllegalTarget()
        {
            Pointer<AbstractClass> pStandTarget;
            if (!(pStandTarget = pStand.Ref.Target).IsNull && !pStand.CanAttack(pStandTarget))
            {
                pStand.ClearAllTarget();
            }
        }

        public void UpdateLocation(LocationMark mark)
        {
            if (null != lastLocationMark && !isMoving)
            {
                isMoving = lastLocationMark.Location != mark.Location;
            }
            lastLocationMark = mark;
            SetLocation(mark.Location);
            SetDirection(mark.Direction, false);
        }

        private void SetLocation(CoordStruct location)
        {
            // Logger.Log("{0} - 移动替身[{1}]{2}到位置{3}", Game.CurrentFrame, Type.Type, pStand, location);
            pStand.Ref.Base.SetLocation(location);
            if (!Data.IsTrain && Data.SameMoving && Data.StickOnFloor && !pStand.Ref.Type.Ref.JumpJet && pMaster.Ref.GetHeight() == 0)
            {
                // 贴在地上
                pStand.Ref.Base.SetHeight(0);
            }
            pStand.Ref.SetFocus(IntPtr.Zero);
        }

        private void SetDirection(DirStruct direction, bool forceSetTurret)
        {
            if (!Data.FreeDirection)
            {
                if (pStand.Ref.HasTurret() || Data.LockDirection)
                {
                    // Logger.Log("设置替身{0}身体的朝向", Type.Type);
                    // 替身有炮塔直接转身体
                    pStand.Ref.Facing.set(direction);
                }
                // 检查是否需要同步转炮塔
                if ((pStand.Ref.Target.IsNull || Data.LockDirection) && !pStand.Ref.Type.Ref.TurretSpins)
                {
                    // Logger.Log("设置替身{0}炮塔的朝向", Type.Type);
                    if (forceSetTurret)
                    {
                        ForceSetFacing(direction);
                    }
                    else
                    {
                        TurnTurretFacing(direction);
                    }
                }
            }

        }

        private void TurnTurretFacing(DirStruct targetDir)
        {
            if (pStand.Ref.HasTurret())
            {
                // 炮塔的旋转交给炮塔旋转自己控制
                if (!pStand.TryGetStatus(out TechnoStatusScript status) || !status.TurretAngleData.Enable)
                {
                    pStand.Ref.TurretFacing.turn(targetDir);
                }
            }
            else
            {
                pStand.Ref.Facing.turn(targetDir);
            }
        }

        private void ForceSetFacing(DirStruct bodyDir)
        {
            pStand.Ref.Facing.set(bodyDir);
            if (pStand.Ref.HasTurret())
            {
                if (pStand.TryGetStatus(out TechnoStatusScript status) && status.DefaultAngleIsChange(bodyDir))
                {
                    pStand.Ref.TurretFacing.set(status.LockTurretDir);
                }
                else
                {
                    pStand.Ref.TurretFacing.set(bodyDir);
                }
            }
        }

        public override void OnPut(Pointer<CoordStruct> pCoord, DirType dirType)
        {
            if (pStand.Ref.Base.InLimbo)
            {
                CoordStruct location = pCoord.Data;
                if (!TryPutStand(location))
                {
                    // Put不出来？
                    Disable(location);
                }
            }
        }

        public override void OnRemove()
        {
            if (!pOwner.IsDead())
            {
                pStand.Ref.Base.Remove();
            }
        }

        public override void OnReceiveDamageDestroy()
        {
            this.onReceiveDamageDestroy = true;
            // 我不做人了JOJO
            notBeHuman = Data.ExplodesWithMaster;
            // Logger.Log($"{Game.CurrentFrame} - 替身 {pStand}[{pStand.Ref.Type.Ref.Base.Base.ID}]的宿主 {pMaster}[{pMaster.Ref.Type.Ref.Base.ID}]被攻击摧毁");
            if (pMaster.CastToTechno(out Pointer<TechnoClass> pTechno))
            {
                // 沉没，坠机，不销毁替身
                pStand.Convert<MissionClass>().Ref.QueueMission(Mission.Sleep, true);
            }
            else if (pMaster.CastToBullet(out Pointer<BulletClass> pBullet))
            {
                // 抛射体上的宿主直接炸
                Disable(pMaster.Ref.Base.GetCoords());
            }
        }

        public override void OnStopCommand()
        {
            // Logger.Log("清空替身{0}的目标对象", Type.Type);
            pStand.ClearAllTarget();
            onStopCommand = true;
            if (!pStand.Ref.Base.IsSelected)
            {
                pStand.Ref.ClickedEvent(EventType.IDLE);
            }
        }


    }
}
