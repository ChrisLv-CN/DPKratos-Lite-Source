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
                GiftBoxState.SaveStatue(pTechno);
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成为盒子，准备开盒");
                if (GiftBoxState.CanOpen() && !GiftBoxState.Data.OpenWhenDestroyed && !GiftBoxState.Data.OpenWhenHealthPercent)
                {
                    // 开盒
                    GiftBoxState.IsOpen = true;
                    // 释放礼物
                    List<string> gifts = GiftBoxState.GetGiftList();
                    if (null != gifts && gifts.Count > 0)
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
            if (!pTechno.IsDeadOrInvisible() && damageState != DamageState.NowDead && GiftBoxState.CanOpen() && GiftBoxState.Data.OpenWhenHealthPercent)
            {
                // 计算血量百分比是否达到开启条件
                double healthPercent = pTechno.Ref.Base.GetHealthPercentage();
                if (healthPercent <= GiftBoxState.Data.OpenHealthPercent)
                {
                    // 开盒
                    GiftBoxState.IsOpen = true;
                    // 释放礼物
                    List<string> gifts = GiftBoxState.GetGiftList();
                    if (null != gifts && gifts.Count > 0)
                    {
                        ReleseGift(gifts, GiftBoxState.Data);
                    }
                }

            }

        }

        public unsafe void OnReceiveDamageDestroy_GiftBox()
        {
            if (GiftBoxState.CanOpen() && GiftBoxState.Data.OpenWhenDestroyed)
            {
                // 开盒
                GiftBoxState.IsOpen = true;
                // 释放礼物
                List<string> gifts = GiftBoxState.GetGiftList();
                if (null != gifts && gifts.Count > 0)
                {
                    ReleseGift(gifts, GiftBoxState.Data);
                }
            }
        }

        private void ReleseGift(List<string> gifts, GiftBoxData data)
        {
            Pointer<HouseClass> pHouse = null != GiftBoxState.AE ? GiftBoxState.AE.pSourceHouse : pTechno.Ref.Owner;
            CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
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
                // 开始投送单位，每生成一个单位就选择一次位置
                foreach (string id in gifts)
                {
                    // 随机选择周边的格子
                    if (data.RandomRange > 0)
                    {
                        int landTypeCategory = pCell.Ref.LandType.Category();
                        CellStruct cell = MapClass.Coord2Cell(location);
                        CellStruct[] cellOffset = new CellSpreadEnumerator((uint)data.RandomRange).ToArray();
                        int max = cellOffset.Count();
                        for (int i = 0; i < max; i++)
                        {
                            int index = MathEx.Random.Next(max - 1);
                            CellStruct offset = cellOffset[index];
                            // Logger.Log("随机获取周围格子索引{0}, 共{1}格, 获取的格子偏移{2}, 单位当前坐标{3}, 第一个格子的坐标{4}, 尝试次数{5}, 当前偏移{6}", index, max, offset, location, MapClass.Cell2Coord(cell + cellOffset[0]), i, cellOffset[i]);
                            if (offset == default)
                            {
                                continue;
                            }
                            if (MapClass.Instance.TryGetCellAt(cell + offset, out Pointer<CellClass> pTargetCell))
                            {
                                if (pTargetCell.Ref.LandType.Category() != landTypeCategory
                                    || (data.EmptyCell && !pTargetCell.Ref.GetContent().IsNull))
                                {
                                    // Logger.Log("获取到的格子被占用, 建筑{0}, 步兵{1}, 载具{2}", !pCell.Ref.GetBuilding().IsNull, !pCell.Ref.GetUnit(false).IsNull, !pCell.Ref.GetInfantry(false).IsNull);
                                    continue;
                                }
                                pCell = pTargetCell;
                                location = pCell.Ref.GetCoordsWithBridge();
                                // Logger.Log("获取到的格子坐标{0}", location);
                                break;
                            }
                        }
                    }
                    // 投送单位
                    Pointer<TechnoClass> pGift = GiftBoxHelper.CreateAndPutTechno(id, pHouse, location, pCell);
                    if (!pGift.IsNull)
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成功释放礼物 [{id}]{pGift}, 位置 {location}");
                        Pointer<TechnoTypeClass> pGiftType = pGift.Ref.Type;
                        TechnoStatusScript giftStatus = pGift.GetStatus();
                        if (data.IsTransform)
                        {
                            // 同步朝向
                            if (pGift.CastIf(AbstractType.Aircraft, out Pointer<AircraftClass> pPlane))
                            {
                                // 飞机朝向使用炮塔朝向
                                pGift.Ref.GetRealFacing().set(GiftBoxState.BodyDir);
                            }
                            else if (pGift.CastToFoot(out Pointer<FootClass> pFoot))
                            {
                                pGift.Ref.Facing.set(GiftBoxState.BodyDir);
                                if (pGift.Ref.HasTurret())
                                {
                                    pGift.Ref.TurretFacing.set(GiftBoxState.BodyDir);
                                }

                                ILocomotion loco = pFoot.Ref.Locomotor;
                                if (loco.ToLocomotionClass().Ref.GetClassID() == LocomotionClass.Jumpjet)
                                {
                                    // JJ朝向是单独的Facing
                                    Pointer<JumpjetLocomotionClass> pLoco = loco.ToLocomotionClass<JumpjetLocomotionClass>();
                                    pLoco.Ref.LocomotionFacing.set(GiftBoxState.BodyDir);
                                }
                            }
                            else
                            {
                                pGift.Ref.Facing.set(GiftBoxState.BodyDir);
                                if (pGift.Ref.HasTurret())
                                {
                                    pGift.Ref.TurretFacing.set(GiftBoxState.BodyDir);
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
                        if (inheritAE)
                        {
                            inheritAE = false;
                            // 继承除了GiftBox之外的状态机
                            InheritedStatsTo(giftStatus);
                            // 继承AE
                            AttachEffectScript giftAEM = pGift.GetComponent<AttachEffectScript>();
                            AttachEffectScript boxAEM = pTechno.GetComponent<AttachEffectScript>();
                            boxAEM.InheritedTo(giftAEM, true);
                            // Logger.Log($"{Game.CurrentFrame} 礼物[{id}]{pGift} 继承盒子 [{section}]{pTechno} 的 AE管理器");
                            // 移除指定的AE
                            giftAEM.Disable(data.RemoveEffects);
                        }

                        // 附加AE
                        if (null != data.AttachEffects)
                        {
                            AttachEffectScript giftAEM = pGift.GetComponent<AttachEffectScript>();
                            giftAEM.Attach(data.AttachEffects);
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
                                        if (!pCell.IsNull)
                                        {
                                            scatterPos = pCell.Ref.GetCoordsWithBridge();
                                        }
                                        pGift.Ref.Base.Scatter(scatterPos, true, false);
                                    }
                                    scatter = true;
                                }
                                else
                                {
                                    if (pGift.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                                    {
                                        CoordStruct des = pDest.IsNull ? location : pDest.Ref.GetCoords();
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
