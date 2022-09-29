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

    [Serializable]
    public class BlackHoleState : State<BlackHoleData>
    {

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

        public void Capture(Pointer<ObjectClass> pObject, Pointer<HouseClass> pHouse)
        {
            BlackHole data = Data.Data;
            if (pObject.CastToTechno(out Pointer<TechnoClass> pTechno) && pTechno.Ref.Veterancy.IsElite())
            {
                data = Data.EliteData;
            }
            if (data.Range > 0)
            {
                CoordStruct location = pObject.Ref.Base.GetCoords();
                if (Data.AffectBullet)
                {
                    // 查找所有的抛射体，并重设目标
                    ExHelper.FindBullet(pHouse, location, data.Range, (pTarget) =>
                    {
                        string id = pTarget.Ref.Type.Ref.Base.Base.ID;
                        // 过滤指定类型
                        if (pObject != pTarget.Convert<ObjectClass>() && Data.CanAffectType(id)
                            && pTarget.TryGetStatus(out BulletStatusScript bulletStatus) && !bulletStatus.LifeData.IsDetonate
                            && !bulletStatus.BlackHoleState.IsActive() // 黑洞不能捕获另一个黑洞
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} 黑洞 [{pObject.Ref.Type.Ref.Base.ID}]{pObject} 捕获抛射体 [{id}]{pTarget}");
                            bulletStatus.SetBlackHole(pObject);
                        }
                        return false;
                    }, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                }
                if (Data.AffectTechno)
                {
                    // 查找所有的单位，并设置黑洞
                    ExHelper.FindTechno(pHouse, location, data.Range, (pTarget) =>
                    {
                        string id = pTarget.Ref.Type.Ref.Base.Base.ID;
                        if (pObject != pTarget.Convert<ObjectClass>() && Data.CanAffectType(id)
                            && pTarget.TryGetStatus(out TechnoStatusScript targetStatus) && !targetStatus.IsBuilding
                            && !targetStatus.BlackHoleState.IsActive() // 黑洞不能捕获另一个黑洞
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} 黑洞 [{pObject.Ref.Type.Ref.Base.ID}]{pObject} 捕获单位 [{id}]{pTarget}");
                            targetStatus.SetBlackHole(pObject);
                        }
                        return false;
                    }, Data.AffectsOwner, Data.AffectsAllies, Data.AffectsEnemies, Data.AffectsCivilian);
                }
            }
            Reload(data.Rate);
        }
    }

}
