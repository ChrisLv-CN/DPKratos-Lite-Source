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

    public partial class TechnoStatusScript
    {

        public GiftBoxState GiftBoxState = new GiftBoxState();

        public void InitState_GiftBox()
        {
            GiftBoxData giftBoxTypeData = Ini.GetConfig<GiftBoxData>(Ini.RulesDependency, section).Data;
            if (null != giftBoxTypeData.Data || null != giftBoxTypeData.EliteData)
            {
                GiftBoxState.Enable(giftBoxTypeData);
            }
        }

        public void OnUpdate_GiftBox()
        {
            GiftBoxState.Update(pTechno.Ref.Veterancy.IsElite());
            if (GiftBoxState.IsActive())
            {
                // 记录盒子的状态
                GiftBoxState.IsSelected = pTechno.Ref.Base.IsSelected;
                GiftBoxState.Group = pTechno.Ref.Group;
                // 记录朝向
                GiftBoxState.BodyDir = pTechno.Ref.Facing.current();
                GiftBoxState.TurretDir = pTechno.Ref.TurretFacing.current();
                // JJ有单独的Facing
                if (isJumpjet)
                {
                    GiftBoxState.BodyDir = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<JumpjetLocomotionClass>().Ref.LocomotionFacing.current();
                    GiftBoxState.TurretDir = GiftBoxState.BodyDir;
                }
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成为盒子，准备开盒");
                if (GiftBoxState.CanOpen() && IsOnMark() && !GiftBoxState.Data.OpenWhenDestroyed && !GiftBoxState.Data.OpenWhenHealthPercent)
                {
                    // 开盒
                    GiftBoxState.IsOpen = true;
                    // 释放礼物
                    List<string> gifts = GiftBoxState.GetGiftList();
                    if (null != gifts && gifts.Any())
                    {
                        ReleseGift(gifts, GiftBoxState.Data);
                    }
                }

                if (GiftBoxState.IsOpen)
                {
                    // 销毁或重置盒子
                    if (GiftBoxState.Data.Remove)
                    {
                        GiftBoxState.Disable();
                        if (GiftBoxState.Data.Destroy)
                        {
                            pTechno.Ref.Base.TakeDamage(pTechno.Ref.Base.Health + 1, pTechno.Ref.Type.Ref.Crewed);
                            // pTechno.Ref.Base.Destroy();
                        }
                        else
                        {
                            pTechno.Ref.Base.Health = 0;
                            pTechno.Ref.Base.Remove();
                            pTechno.Ref.Base.UnInit();
                        }
                    }
                    else
                    {
                        // 重置
                        GiftBoxState.Reset();
                    }
                }
            }
        }

        public unsafe void OnReceiveDamage2_GiftBox(Pointer<int> pRealDamage, Pointer<WarheadTypeClass> pWH, DamageState damageState, Pointer<ObjectClass> pAttacker, Pointer<HouseClass> pAttackingHouse)
        {
            if (!pTechno.IsDeadOrInvisible() && damageState != DamageState.NowDead && GiftBoxState.CanOpen() && IsOnMark() && GiftBoxState.Data.OpenWhenHealthPercent)
            {
                // 计算血量百分比是否达到开启条件
                double healthPercent = pTechno.Ref.Base.GetHealthPercentage();
                if (healthPercent <= GiftBoxState.Data.OpenHealthPercent)
                {
                    // 开盒
                    GiftBoxState.IsOpen = true;
                    // 释放礼物
                    List<string> gifts = GiftBoxState.GetGiftList();
                    if (null != gifts && gifts.Any())
                    {
                        ReleseGift(gifts, GiftBoxState.Data);
                    }
                }

            }

        }

        public unsafe void OnReceiveDamageDestroy_GiftBox()
        {
            if (GiftBoxState.CanOpen() && IsOnMark() && GiftBoxState.Data.OpenWhenDestroyed)
            {
                // 开盒
                GiftBoxState.IsOpen = true;
                // 释放礼物
                List<string> gifts = GiftBoxState.GetGiftList();
                if (null != gifts && gifts.Any())
                {
                    ReleseGift(gifts, GiftBoxState.Data);
                }
            }
        }

        private bool IsOnMark()
        {
            string[] marks = GiftBoxState.Data.OnlyOpenWhenMarks;
            return null == marks || !marks.Any() || (pTechno.TryGetAEManager(out AttachEffectScript aeManager) && aeManager.IsOnMark(marks));
        }

        private void ReleseGift(List<string> gifts, GiftBoxData data)
        {
            Pointer<HouseClass> pHouse = IntPtr.Zero;
            if (null != GiftBoxState.AE && !GiftBoxState.AE.AEData.ReceiverOwn)
            {
                pHouse = GiftBoxState.AE.pSourceHouse;
            }
            else
            {
                pHouse = pTechno.Ref.Owner;
            }
            CoordStruct location = default;
            if (data.RealCoords)
            {
                location = pTechno.Ref.Base.GetRenderCoords();
            }
            else
            {
                location = pTechno.Ref.Base.Base.GetCoords();
            }
            Mission curretMission = pTechno.Convert<MissionClass>().Ref.CurrentMission;

            // 获取投送单位的位置
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {
                // 投送后需要前往的目的地
                Pointer<AbstractClass> pDest = IntPtr.Zero; // 载具当前的移动目的地
                Pointer<AbstractClass> pFocus = IntPtr.Zero; // 步兵的移动目的地
                // 获取目的地
                if (pTechno.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                {
                    pDest = pTechno.Convert<FootClass>().Ref.Destination;
                    pFocus = pTechno.Ref.Focus;
                }
                // 获取盒子的一些状态
                int healthNumber = pTechno.Ref.Base.Health;
                double healthPercent = pTechno.Ref.Base.GetHealthPercentage();
                healthPercent = healthPercent <= 0 ? 1 : healthPercent; // 盒子死了，继承的血量就是满的
                bool changeHealth = data.IsTransform || data.InheritHealth; // Transform强制继承
                if (!changeHealth && data.HealthPercent > 0)
                {
                    // 强设比例
                    healthPercent = data.HealthPercent;
                    changeHealth = true;
                }
                Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                Mission mission = pTechno.Convert<MissionClass>().Ref.CurrentMission;
                bool scatter = !data.Remove || data.ForceMission == Mission.Move;
                bool inheritAE = data.Remove && data.InheritAE;
                // 随机投送位置
                CellStruct cellPos = pCell.Ref.MapCoords;
                CellStruct[] cellOffsets = null;
                if (data.RandomRange > 0)
                {
                    cellOffsets = new CellSpreadEnumerator((uint)data.RandomRange).ToArray();
                }
                // 开始投送单位，每生成一个单位就选择一次位置
                foreach (string id in gifts)
                {
                    // 投送单位
                    if (GiftBoxHelper.ReleseGift(id, pHouse, location, pCell, cellPos, cellOffsets, data.EmptyCell, out Pointer<TechnoTypeClass> pGiftType, out Pointer<TechnoClass> pGift, out CoordStruct putLocation, out Pointer<CellClass> putCell))
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成功释放礼物 [{id}]{pGift}, 位置 {location}");
                        TechnoStatusScript giftStatus = pGift.GetStatus();
                        if (data.IsTransform)
                        {
                            // 同步朝向
                            pGift.Ref.Facing.set(GiftBoxState.BodyDir);
                            pGift.Ref.TurretFacing.set(GiftBoxState.TurretDir);
                            // JJ朝向是单独的Facing
                            if (pGift.CastToFoot(out Pointer<FootClass> pFoot))
                            {
                                ILocomotion loco = pFoot.Ref.Locomotor;
                                if (loco.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Jumpjet)
                                {
                                    Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                                    pLoco.Ref.LocomotionFacing.set(GiftBoxState.BodyDir);
                                }
                            }
                            // 同步小队
                            pGift.Ref.Group = GiftBoxState.Group;

                            // 同步箱子属性
                            giftStatus.CrateBuff = this.CrateBuff.Clone();
                            // giftExt.CrateStatus += this.CrateStatus;
                        }

                        // 同步选中
                        if (GiftBoxState.IsSelected)
                        {
                            giftStatus.DisableSelectVoice = true;
                            pGift.Ref.Base.Select();
                            giftStatus.DisableSelectVoice = false;
                        }

                        // 修改血量
                        if (changeHealth)
                        {
                            int strength = pGiftType.Ref.Base.Strength;
                            int health = 0;
                            if (data.InheritHealthNumber)
                            {
                                // 直接继承血量数字
                                health = healthNumber;
                            }
                            else if (data.HealthNumber > 0)
                            {
                                // 直接赋予指定血量
                                health = data.HealthNumber;
                            }
                            else
                            {
                                // 按比例计算血量
                                health = (int)(strength * healthPercent);
                            }
                            // Logger.Log($"{Game.CurrentFrame} - 设置礼物 {pGift} [{pGift.Ref.Type.Ref.Base.Base.ID}] 的血量 {health} / {strength} {healthPercent}");
                            if (health <= 0)
                            {
                                health = 1;
                            }
                            if (health < strength)
                            {
                                pGift.Ref.Base.Health = health;
                            }
                        }

                        // 转移乘客
                        // if (data.InheritPassenger && pTechno.Ref.Passengers.NumPassengers > 0)
                        // {
                        //     // 检查乘员舱空间
                        //     int technoSize = pTechno.Ref.Type.Ref.Passengers;
                        //     int giftSize = pGift.Ref.Type.Ref.Passengers;
                        //     Logger.Log($"{Game.CurrentFrame} 转移乘客，礼盒乘客容量{technoSize}，礼物乘客容量{giftSize}，乘客数量{pTechno.Ref.Passengers.NumPassengers}");
                        //     PassengersClass pTechnoPC = pTechno.Ref.Passengers;
                        //     PassengersClass pGiftPC = pGift.Ref.Passengers;
                        //     for (int i = 0; i <= pTechnoPC.NumPassengers; i++)
                        //     {
                        //         Pointer<FootClass> pFoot = pTechnoPC.RemoveFirstPassenger();
                        //         pGiftPC.AddPassenger(pFoot);
                        //         Logger.Log($"{Game.CurrentFrame} 写入{i}个乘客 {pFoot}");
                        //     }
                        // }

                        // 继承等级
                        if (data.InheritExperience && pGiftType.Ref.Trainable)
                        {
                            pGift.Ref.Veterancy = pTechno.Ref.Veterancy;
                        }

                        // 继承ROF
                        if (data.InheritROF && pTechno.Ref.ROFTimer.InProgress())
                        {
                            pGift.Ref.ROFTimer.Start(pTechno.Ref.ROFTimer.GetTimeLeft());
                        }

                        // 继承弹药
                        if (data.InheritAmmo && pGiftType.Ref.Ammo > 1 && pTechno.Ref.Type.Ref.Ammo > 1)
                        {
                            int ammo = pTechno.Ref.Ammo;
                            if (ammo >= 0)
                            {
                                pGift.Ref.Ammo = ammo;
                            }
                        }

                        // 继承AE管理器
                        AttachEffectScript giftAEM = null;
                        if (inheritAE)
                        {
                            inheritAE = false;
                            // 继承除了GiftBox之外的状态机
                            InheritedStatsTo(giftStatus);
                            // 继承AE
                            giftAEM = pGift.GetAEManegr();
                            AttachEffectScript boxAEM = pTechno.GetAEManegr();
                            if (null != giftAEM && null != boxAEM)
                            {
                                boxAEM.InheritedTo(giftAEM, true);
                                // Logger.Log($"{Game.CurrentFrame} 礼物[{id}]{pGift} 继承盒子 [{section}]{pTechno} 的 AE管理器");
                                // 移除指定的AE
                                giftAEM.Disable(data.RemoveEffects);
                            }
                        }

                        // 附加AE
                        if (null != data.AttachEffects)
                        {
                            if (null == giftAEM)
                            {
                                giftAEM = pGift.GetAEManegr();
                            }
                            if (null != giftAEM)
                            {
                                giftAEM.Attach(data.AttachEffects, data.AttachChances);
                            }
                        }

                        if (data.ForceMission != Mission.None && data.ForceMission != Mission.Move)
                        {
                            // 强制任务
                            pGift.Convert<MissionClass>().Ref.QueueMission(data.ForceMission, false);
                        }
                        else
                        {
                            if (!pTarget.IsNull && data.InheritTarget && pGift.Ref.CanAttack(pTarget))
                            {
                                // 同步目标
                                pGift.Ref.SetTarget(pTarget);
                                pGift.Convert<MissionClass>().Ref.QueueMission(curretMission, false);
                            }
                            else
                            {
                                // 开往预定目的地
                                if (pDest.IsNull && pFocus.IsNull)
                                {
                                    // 第一个傻站着，第二个之后的散开
                                    if (scatter || pGiftType.Ref.BalloonHover)
                                    {
                                        // 分散到所在的格子里
                                        CoordStruct scatterPos = CoordStruct.Empty;
                                        if (!putCell.IsNull)
                                        {
                                            scatterPos = putCell.Ref.GetCoordsWithBridge();
                                        }
                                        pGift.Ref.Base.Scatter(scatterPos, true, false);
                                    }
                                    scatter = true;
                                }
                                else
                                {
                                    if (pGift.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                                    {
                                        CoordStruct des = pDest.IsNull ? putLocation : pDest.Ref.GetCoords();
                                        if (!pFocus.IsNull)
                                        {
                                            pGift.Ref.SetFocus(pFocus);
                                            if (pGift.Ref.Base.Base.WhatAmI() == AbstractType.Unit)
                                            {
                                                des = pFocus.Ref.GetCoords();
                                            }
                                        }
                                        if (MapClass.Instance.TryGetCellAt(des, out Pointer<CellClass> pTargetCell))
                                        {
                                            pGift.Ref.SetDestination(pTargetCell, true);
                                            pGift.Convert<MissionClass>().Ref.QueueMission(Mission.Move, true);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Logger.LogWarning("Gift box release gift failed, unknown TechnoType [{0}]", id);
                    }
                }
            }
        }
    }
}
