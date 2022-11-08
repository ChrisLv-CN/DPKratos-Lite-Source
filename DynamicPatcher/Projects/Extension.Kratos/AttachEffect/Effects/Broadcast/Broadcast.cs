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
                    FinderHelper.FindTechnoOnMark((pTarget, aeManager) =>
                    {
                        // 赋予AE
                        aeManager.Attach(data.Types, pOwner);
                        return false;
                    }, location, data.RangeMax, data.RangeMin, pHouse, Data, pOwner);
                }
                // 搜索抛射体
                if (Data.AffectBullet)
                {
                    FinderHelper.FindBulletOnMark((pTarget, aeManager) =>
                    {
                        // 赋予AE
                        aeManager.Attach(data.Types, pOwner);
                        return false;
                    }, location, data.RangeMax, data.RangeMin, pHouse, Data, pOwner);
                }
            }
        }
    }
}
