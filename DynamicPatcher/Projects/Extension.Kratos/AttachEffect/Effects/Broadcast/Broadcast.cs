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
                CoordStruct location = pOwner.Ref.Base.GetCoords();
                double cellSpread = data.RangeMax;

                // 搜索单位
                if (Data.AffectTechno)
                {
                    List<Pointer<TechnoClass>> pTechnoList = null;
                    if (cellSpread <= 0)
                    {
                        // 搜索全部单位
                        HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();
                        if (Data.AffectBuilding)
                        {
                            BuildingClass.Array.FindObject((pTarget) =>
                            {
                                if (!pTarget.Ref.Type.Ref.InvisibleInGame)
                                {
                                    pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                                }
                                return false;
                            }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        }
                        if (Data.AffectInfantry)
                        {
                            InfantryClass.Array.FindObject((pTarget) =>
                            {
                                Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                                if (!Data.AffectsAir || !pTargetTechno.InAir())
                                {
                                    pTechnoSet.Add(pTargetTechno);
                                }
                                return false;
                            }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        }
                        if (Data.AffectUnit)
                        {
                            UnitClass.Array.FindObject((pTarget) =>
                            {
                                Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                                if (!Data.AffectsAir || !pTargetTechno.InAir())
                                {
                                    pTechnoSet.Add(pTargetTechno);
                                }
                                return false;
                            }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        }
                        if (Data.AffectAircraft)
                        {
                            AircraftClass.Array.FindObject((pTarget) =>
                            {
                                Pointer<TechnoClass> pTargetTechno = pTarget.Convert<TechnoClass>();
                                if (!Data.AffectsAir || !pTargetTechno.InAir())
                                {
                                    pTechnoSet.Add(pTargetTechno);
                                }
                                return false;
                            }, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                        }
                        pTechnoList = new List<Pointer<TechnoClass>>(pTechnoSet);
                    }
                    else
                    {
                        // 小范围搜索
                        pTechnoList = FinderHelper.GetCellSpreadTechnos(location, cellSpread, Data.AffectsAir, false, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    }
                    if (null != pTechnoList)
                    {
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
                            // 检查最小距离
                            if (data.RangeMin > 0)
                            {
                                double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                                if (pTarget.InAir() && pTarget.Ref.Base.Base.WhatAmI() == AbstractType.Aircraft)
                                {
                                    distance *= 0.5;
                                }
                                if (distance < data.RangeMin * 256)
                                {
                                    continue;
                                }
                            }
                            // 可影响
                            if (Data.CanAffectType(pTarget) && pTarget.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager))
                            {
                                // 赋予AE
                                aeManager.Attach(data.Types, pOwner);
                            }
                        }
                    }
                }
                // 搜索抛射体
                if (Data.AffectBullet)
                {
                    HashSet<Pointer<BulletClass>> pBulletSet = new HashSet<Pointer<BulletClass>>();
                    BulletClass.Array.FindObject((pTarget) =>
                    {
                        if (!pTarget.IsDeadOrInvisible() && pTarget.Convert<ObjectClass>() != pOwner)
                        {
                            if (data.RangeMin > 0)
                            {
                                double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                                if (distance < data.RangeMin * 256)
                                {
                                    return false;
                                }
                            }
                            // 可影响
                            if (Data.CanAffectType(pTarget))
                            {
                                pBulletSet.Add(pTarget);
                            }
                        }
                        return false;
                    }, location, cellSpread, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    // 赋予AE
                    foreach (Pointer<BulletClass> pBullet in pBulletSet)
                    {
                        if (pBullet.TryGetAEManager(out AttachEffectScript aeManager) && IsOnMark(aeManager))
                        {
                            aeManager.Attach(data.Types, pOwner);
                        }
                    }
                }
            }
        }

        private bool IsOnMark(AttachEffectScript aeManager)
        {
            return null == Data.OnlyAffectMarks || !Data.OnlyAffectMarks.Any()
                || (aeManager.TryGetMarks(out HashSet<string> marks)
                    && (Data.OnlyAffectMarks.Intersect(marks).Count() > 0)
                );
        }

    }
}
