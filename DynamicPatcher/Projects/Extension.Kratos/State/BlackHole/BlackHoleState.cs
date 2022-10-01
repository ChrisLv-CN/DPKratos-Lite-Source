using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{
    public interface IBlackHoleVictim
    {
        void SetBlackHole(Pointer<ObjectClass> pBlackHole, BlackHoleData data);
    }

    [Serializable]
    public class BlackHoleState : State<BlackHoleData>
    {
        private bool isElite;

        private int count;
        private int delay;
        private TimerStruct delayTimer;

        public override void OnEnable()
        {
            this.count = 0;
        }

        private void Reload(int delay)
        {
            this.delay = delay;
            if (delay > 0)
            {
                this.delayTimer.Start(delay);
            }
            count++;
            if (IsDone())
            {
                Disable();
            }
        }

        public bool IsReady()
        {
            return IsActive() && !IsDone() && Timeup();
        }

        private bool Timeup()
        {
            return this.delay <= 0 || delayTimer.Expired();
        }

        private bool IsDone()
        {
            return Data.Count > 0 && count >= Data.Count;
        }

        private BlackHole GetData()
        {
            if (isElite)
            {
                return Data.EliteData;
            }
            return Data.Data;
        }

        public void StartCapture(Pointer<ObjectClass> pBlackHole, Pointer<HouseClass> pHouse)
        {
            this.isElite = pBlackHole.CastToTechno(out Pointer<TechnoClass> pTechno) && pTechno.Ref.Veterancy.IsElite();
            BlackHole data = GetData();
            if (null != data && data.Range > 0)
            {
                CoordStruct location = pBlackHole.Ref.Base.GetCoords();
                if (Data.AffectBullet)
                {
                    // 查找所有的抛射体，并重设目标

                    BulletClass.Array.FindObject((pTarget) =>
                    {
                        string id = pTarget.Ref.Type.Ref.Base.Base.ID;
                        // 过滤指定类型
                        if (pBlackHole != pTarget.Convert<ObjectClass>() && Data.CanAffectType(pTarget)
                            && pTarget.TryGetStatus(out BulletStatusScript bulletStatus) && !bulletStatus.LifeData.IsDetonate
                        // && !bulletStatus.BlackHoleState.IsActive() // 黑洞不能捕获另一个黑洞
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} 黑洞 [{pObject.Ref.Type.Ref.Base.ID}]{pObject} 捕获抛射体 [{id}]{pTarget}");
                            bulletStatus.SetBlackHole(pBlackHole, Data);
                        }
                        return false;
                    }, location, data.Range, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                }
                if (Data.AffectTechno)
                {
                    HashSet<Pointer<TechnoClass>> pTechnoSet = new HashSet<Pointer<TechnoClass>>();
                    if (Data.AffectBuilding)
                    {
                        // 查找所有的建筑
                        BuildingClass.Array.FindObject((pTarget) =>
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                            return false;
                        }, location, data.Range, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    }
                    if (Data.AffectInfantry)
                    {
                        // 查找所有的步兵
                        InfantryClass.Array.FindObject((pTarget) =>
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                            return false;
                        }, location, data.Range, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    }
                    if (Data.AffectUnit)
                    {
                        // 查找所有的载具
                        UnitClass.Array.FindObject((pTarget) =>
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                            return false;
                        }, location, data.Range, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    }
                    if (Data.AffectAircraft)
                    {
                        // 查找所有的飞机
                        AircraftClass.Array.FindObject((pTarget) =>
                        {
                            pTechnoSet.Add(pTarget.Convert<TechnoClass>());
                            return false;
                        }, location, data.Range, pHouse, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                    }
                    // 筛查所有的单位，并设置黑洞
                    foreach (Pointer<TechnoClass> pTarget in pTechnoSet)
                    {
                        if (pBlackHole != pTarget.Convert<ObjectClass>() && Data.CanAffectType(pTarget)
                            && (Data.Weight <= 0 || pTarget.Ref.Type.Ref.Weight <= Data.Weight) // 排除质量较大的对象
                            && pTarget.TryGetStatus(out TechnoStatusScript targetStatus)
                        // && !targetStatus.BlackHoleState.IsActive() // 黑洞不能捕获另一个黑洞
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} 黑洞 [{pObject.Ref.Type.Ref.Base.ID}]{pObject} 捕获单位 [{id}]{pTarget}");
                            targetStatus.SetBlackHole(pBlackHole, Data);
                        }
                    }
                }

                Reload(data.Rate);
            }
        }

        public bool IsOutOfRange(double distance)
        {
            BlackHole data = GetData();
            return null == data || data.Range <= 0 || distance > data.Range * 256;
        }

    }

}
