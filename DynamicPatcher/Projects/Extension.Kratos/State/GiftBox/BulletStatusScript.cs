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

    public partial class BulletStatusScript
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
            if (!pBullet.Ref.Owner.IsNull)
            {
                Pointer<TechnoClass> pTechno = pBullet.Ref.Owner;
                GiftBoxState.Update(pTechno.Ref.Veterancy.IsElite());
                GiftBoxState.IsSelected = pTechno.Ref.Base.IsSelected;
                GiftBoxState.Group = pTechno.Ref.Group;
            }
            if (GiftBoxState.IsActive())
            {
                // 子弹的方向
                GiftBoxState.BodyDir = pBullet.Facing();
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成为盒子，准备开盒");
                if (!GiftBoxState.Data.OpenWhenDestroyed && !GiftBoxState.Data.OpenWhenHealthPercent && GiftBoxState.CanOpen())
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
                        bool harmless = !GiftBoxState.Data.Destroy;
                        LifeData.Detonate(harmless);
                    }
                    else
                    {
                        // 重置
                        GiftBoxState.Reset();
                    }
                }
            }
        }

        public unsafe bool OnDetonate_GiftBox(Pointer<CoordStruct> pCoords)
        {
            if (GiftBoxState.IsActive() && GiftBoxState.Data.OpenWhenDestroyed && !GiftBoxState.IsOpen)
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
            return false;
        }

        private void ReleseGift(List<string> gifts, GiftBoxData data)
        {
            Pointer<HouseClass> pHouse = pSourceHouse;
            CoordStruct location = pBullet.Ref.Base.Base.GetCoords();
            // 获取投送单位的位置
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {

                // 投送后需要前往的目的地
                Pointer<AbstractClass> pTarget = pBullet.Ref.Target;
                Pointer<AbstractClass> pFocus = IntPtr.Zero; // 步兵的移动目的地

                // 取目标所在位置
                CoordStruct targetLocation = pBullet.Ref.TargetCoords;
                if (!pTarget.IsNull)
                {
                    targetLocation = pTarget.Ref.GetCoords();
                }
                // 取目标位置所在的格子作为移动目的地
                if (MapClass.Instance.TryGetCellAt(targetLocation, out Pointer<CellClass> pTargetLocationCell))
                {
                    pFocus = pTargetLocationCell.Convert<AbstractClass>(); // 步兵的移动目的地
                }

                bool scatter = !data.Remove || data.ForceMission == Mission.Move;
                // 随机投送位置
                CellStruct cell = pCell.Ref.MapCoords;
                CellStruct[] cellOffset = null;
                int max = 0;
                if (data.RandomRange > 0)
                {
                    cellOffset = new CellSpreadEnumerator((uint)data.RandomRange).ToArray();
                    max = cellOffset.Count();
                }
                // 开始投送单位，每生成一个单位就选择一次位置
                foreach (string id in gifts)
                {
                    Pointer<TechnoTypeClass> pType = IntPtr.Zero;
                    if (id.IsNullOrEmptyOrNone() || (pType = TechnoTypeClass.ABSTRACTTYPE_ARRAY.Find(id)).IsNull)
                    {
                        continue;
                    }
                    // 随机选择周边的格子
                    CoordStruct putLocation = location;
                    Pointer<CellClass> putCell = pCell;
                    for (int i = 0; i < max; i++)
                    {
                        int index = MathEx.Random.Next(max);
                        CellStruct offset = cellOffset[index];
                        // Logger.Log($"{Game.CurrentFrame} 随机获取周围格子索引{index}, 共{max}格, 尝试次数{i}, 获取的格子偏移{offset}, 单位当前坐标{cell}, 当前偏移{cellOffset[i]}, 偏移量[{string.Join(",", cellOffset)}]");
                        if (MapClass.Instance.TryGetCellAt(cell + offset, out Pointer<CellClass> pTargetCell))
                        {
                            BulletEffectHelper.GreenCell(pTargetCell.Ref.GetCoordsWithBridge());
                            if (!data.EmptyCell || pTargetCell.Ref.IsClearToMove(pType.Ref.SpeedType, pType.Ref.MovementZone))
                            {
                                putCell = pTargetCell;
                                putLocation = pTargetCell.Ref.GetCoordsWithBridge();
                                // Logger.Log($"{Game.CurrentFrame} 获取到的格子坐标{cell + offset}");
                                break;
                            }
                        }
                    }
                    // 投送单位
                    Pointer<TechnoClass> pGift = GiftBoxHelper.CreateAndPutTechno(pType, pHouse, putLocation, putCell);
                    if (!pGift.IsNull)
                    {
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 成功释放礼物 [{id}]{pGift}, 位置 {location}");
                        Pointer<TechnoTypeClass> pGiftType = pGift.Ref.Type;
                        TechnoStatusScript giftStatus = pGift.GetStatus();

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

                        // 附加AE
                        if (null != data.AttachEffects)
                        {
                            AttachEffectScript giftAEM = pGift.GetComponent<AttachEffectScript>();
                            giftAEM.Attach(data.AttachEffects, data.AttachChances);
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
                                pGift.Convert<MissionClass>().Ref.QueueMission(Mission.Attack, false);
                            }
                            else
                            {
                                // 开往预定目的地
                                if (pFocus.IsNull)
                                {
                                    // 第一个傻站着，第二个之后的散开
                                    if (scatter || pGiftType.Ref.BalloonHover)
                                    {
                                        pGift.Ref.Base.Scatter(CoordStruct.Empty, true, false);
                                    }
                                    scatter = true;
                                }
                                else if (pGift.Ref.Base.Base.WhatAmI() != AbstractType.Building)
                                {
                                    if (!pFocus.IsNull)
                                    {
                                        pGift.Ref.SetFocus(pFocus);
                                        if (pGift.Ref.Base.Base.WhatAmI() == AbstractType.Unit)
                                        {
                                            pGift.Ref.SetDestination(pTargetLocationCell, true);
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
