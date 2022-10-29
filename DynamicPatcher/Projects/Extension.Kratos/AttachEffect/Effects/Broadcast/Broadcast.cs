using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
        public Broadcast Broadcast;

        private void InitBroadcast()
        {
            this.Broadcast = AEData.BroadcastData.CreateEffect<Broadcast>();
            RegisterEffect(Broadcast);
        }
    }


    [Serializable]
    public class Broadcast : Effect<BroadcastData>
    {
        private TimerStruct delayTimer;
        private int count;

        public override void OnUpdate(CoordStruct location, bool isDead)
        {
            if (!isDead)
            {
                BroadcastEntity data = Data.Data;
                Pointer<HouseClass> pHouse = IntPtr.Zero;
                if (pOwner.CastToTechno(out Pointer<TechnoClass> pTechno))
                {
                    pHouse = pTechno.Ref.Owner;
                    if (pTechno.Ref.Veterancy.IsElite())
                    {
                        data = Data.EliteData;
                    }
                }
                else if (pOwner.CastToBullet(out Pointer<BulletClass> pBullet))
                {
                    pHouse = pBullet.GetSourceHouse();
                }
                else
                {
                    return;
                }

                // 检查平民
                if (Data.DeactiveWhenCivilian && pHouse.IsCivilian())
                {
                    return;
                }
                if (null != data)
                {
                    if (delayTimer.Expired())
                    {
                        // 检查次数
                        if (Data.Count > 0 && ++count > Data.Count)
                        {
                            Disable(default);
                        }
                        delayTimer.Start(data.Rate);
                        FindAndAttach(data, pHouse);
                    }
                }
            }
        }

        public void FindAndAttach(BroadcastEntity data, Pointer<HouseClass> pHouse)
        {
            if (null != data.Types && data.Types.Length > 0)
            {
                bool findTechno = false;
                bool findBullet = false;
                // 快速检索是否需要查找单位或者抛射体清单
                foreach (string ae in data.Types)
                {
                    AttachEffectData aeData = Ini.GetConfig<AttachEffectData>(Ini.RulesDependency, ae).Data;
                    findTechno |= aeData.AffectTechno;
                    findBullet |= aeData.AffectBullet;
                }

                CoordStruct location = pOwner.Ref.Base.GetCoords();
                double cellSpread = data.RangeMax;

                if (findTechno)
                {
                    if (cellSpread <= 0)
                    {
                        HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();
                        // 搜索所有的单位类型
                        BuildingClass.Array.FindObject((pTarget) =>
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                            return false;
                        }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        FinderHelper.FindFoot((pFoot) =>
                        {
                            pTechnoSet.Add(pFoot.Convert<TechnoClass>());
                            return false;
                        }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        // 过滤掉不可以用的类型
                        List<Pointer<TechnoClass>> pTechnoList = new List<Pointer<TechnoClass>>();
                        foreach (Pointer<TechnoClass> pTechno in pTechnoSet)
                        {
                            if (!Data.AffectsAir && pTechno.InAir())
                            {
                                continue;
                            }
                            CoordStruct targetPos = pTechno.Ref.Base.Base.GetCoords();
                            double dist = targetPos.DistanceFrom(location);
                            AbstractType absType = pTechno.Ref.Base.Base.WhatAmI();
                            switch (absType)
                            {
                                case AbstractType.Building:
                                    if (pTechno.Convert<BuildingClass>().Ref.Type.Ref.InvisibleInGame)
                                    {
                                        continue;
                                    }
                                    break;
                                case AbstractType.Aircraft:
                                    if (pTechno.InAir())
                                    {
                                        if (!Data.AffectsAir)
                                        {
                                            continue;
                                        }
                                        dist *= 0.5;
                                    }
                                    break;
                            }
                            // 检查最小距离
                            if (data.RangeMin > 0)
                            {
                                if (dist < data.RangeMin * 256)
                                {
                                    continue;
                                }
                            }
                            pTechnoList.Add(pTechno);
                        }
                        foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                        {
                            // 赋予AE
                            if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                            {
                                aeManager.Attach(data.Types, pOwner);
                            }
                        }
                    }
                    else
                    {
                        // 检索范围内的单位类型
                        List<Pointer<TechnoClass>> pTechnoList = FinderHelper.GetCellSpreadTechnos(location, cellSpread, Data.AffectsAir, false);
                        foreach (Pointer<TechnoClass> pTarget in pTechnoList)
                        {
                            // 检查死亡
                            if (pTarget.IsDeadOrInvisible() || pTarget.Convert<ObjectClass>() == pOwner)
                            {
                                continue;
                            }
                            // 过滤替身和虚单位
                            if (pTarget.TryGetStatus(out var status) && (!status.MyMaster.IsNull || status.MyMasterIsAnim || status.VirtualUnit))
                            {
                                continue;
                            }
                            if (data.RangeMin > 0)
                            {
                                double distance = location.DistanceFrom(pTarget.Ref.Base.Location);
                                if (distance < data.RangeMin * 256)
                                {
                                    continue;
                                }
                            }
                            Pointer<HouseClass> pTargetHouse = pTarget.Ref.Owner;
                            // 可影响
                            if (!pHouse.IsNull && !pTargetHouse.IsNull
                                && ((pTargetHouse.IsCivilian() && Data.AffectsCivilian)
                                    || (pTargetHouse == pHouse ? Data.AffectsOwner : (pTargetHouse.Ref.IsAlliedWith(pHouse) ? Data.AffectsAllies : Data.AffectsEnemies))
                                )
                            )
                            {
                                // 赋予AE
                                if (pTarget.TryGetAEManager(out AttachEffectScript aeManager))
                                {
                                    aeManager.Attach(data.Types, pOwner);
                                }
                            }
                        }
                    }
                }

                // 检索爆炸范围内的抛射体类型
                if (findBullet)
                {
                    HashSet<Pointer<BulletClass>> pBulletSet = new HashSet<Pointer<BulletClass>>();
                    BulletClass.Array.FindObject((pTarget) =>
                    {
                        if (!pTarget.IsDeadOrInvisible() && pTarget.Convert<ObjectClass>() != pOwner)
                        {
                            if (data.RangeMin > 0)
                            {
                                double distance = location.DistanceFrom(pTarget.Ref.Base.Location);
                                if (distance < data.RangeMin * 256)
                                {
                                    return false;
                                }
                            }
                            // 可影响
                            pBulletSet.Add(pTarget);
                        }
                        return false;
                    }, location, cellSpread, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    // 赋予AE
                    foreach (Pointer<BulletClass> pBullet in pBulletSet)
                    {
                        if (pBullet.TryGetAEManager(out AttachEffectScript aeManager))
                        {
                            aeManager.Attach(data.Types, pOwner);
                        }
                    }
                }
            }
        }

    }
}
