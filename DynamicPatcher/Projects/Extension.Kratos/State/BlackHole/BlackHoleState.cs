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
        void SetBlackHole(IExtension blackHoleExt, BlackHoleData data);
    }

    [Serializable]
    public class BlackHoleState : State<BlackHoleData>
    {

        public IExtension Owner;

        private bool isElite;

        private int count;
        private int delay;
        private TimerStruct delayTimer;

        public override void OnEnable()
        {
            this.count = 0;
            if (null != AE)
            {
                Owner = AE.AEManager.Owner;
            }
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
            return Data.TriggeredTimes > 0 && count >= Data.TriggeredTimes;
        }

        public BlackHoleEntity GetData()
        {
            if (isElite)
            {
                return Data.EliteData;
            }
            return Data.Data;
        }

        public void StartCapture(IExtension blackHoleExt, Pointer<HouseClass> pHouse)
        {
            this.Owner = blackHoleExt;
            Pointer<ObjectClass> pBlackHole = blackHoleExt.OwnerObject;
            this.isElite = pBlackHole.CastToTechno(out Pointer<TechnoClass> pTechno) && pTechno.Ref.Veterancy.IsElite();
            BlackHoleEntity data = GetData();
            if (!Data.DontScan && null != data && data.Range != 0)
            {
                Reload(data.Rate);
                // ????????????
                if (Data.DeactiveWhenCivilian && pHouse.IsCivilian())
                {
                    return;
                }
                CoordStruct location = pBlackHole.Ref.Base.GetCoords();
                if (Data.AffectBullet)
                {
                    // ????????????????????????
                    FinderHelper.FindBulletOnMark((pTarget, aem) =>
                    {
                        if (pTarget.TryGetStatus(out BulletStatusScript bulletStatus) && !bulletStatus.LifeData.IsDetonate
                            && (Data.AffectBlackHole || !bulletStatus.BlackHoleState.IsActive()) // ?????????????????????????????????
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} ?????? [{pObject.Ref.Type.Ref.Base.ID}]{pObject} ??????????????? [{pTarget.Ref.Type.Ref.Base.Base.ID}]{pTarget}");
                            bulletStatus.SetBlackHole(blackHoleExt, Data);
                        }
                        return false;
                    }, location, data.Range, 0, data.FullAirspace, pHouse, Data, pBlackHole);
                }
                if (Data.AffectTechno)
                {
                    // ?????????????????????
                    FinderHelper.FindTechnoOnMark((pTarget, aem) =>
                    {
                        if ((Data.Weight <= 0 || pTarget.Ref.Type.Ref.Weight <= Data.Weight) // ???????????????????????????
                            && pTarget.TryGetStatus(out TechnoStatusScript targetStatus)
                            && (Data.AffectBlackHole || !targetStatus.BlackHoleState.IsActive()) // ?????????????????????????????????
                        )
                        {
                            // Logger.Log($"{Game.CurrentFrame} ?????? [{pObject.Ref.Type.Ref.Base.ID}]{pObject} ???????????? [{id}]{pTarget}");
                            targetStatus.SetBlackHole(blackHoleExt, Data);
                        }
                        return false;
                    }, location, data.Range, 0, data.FullAirspace, pHouse, Data, pBlackHole);
                }
            }
        }

        public bool IsOutOfRange(double distance)
        {
            BlackHoleEntity data = GetData();
            return null == data || data.Range == 0 || (data.Range > 0 && distance > data.Range * 256);
        }

        public bool IsOnMark(Pointer<BulletClass> pTarget)
        {
            return null == Data.OnlyAffectMarks || !Data.OnlyAffectMarks.Any()
                || (pTarget.TryGetAEManager(out AttachEffectScript aem)
                    && aem.TryGetMarks(out HashSet<string> marks)
                    && (Data.OnlyAffectMarks.Intersect(marks).Count() > 0)
                );
        }

        public bool IsOnMark(Pointer<TechnoClass> pTarget)
        {
            return null == Data.OnlyAffectMarks || !Data.OnlyAffectMarks.Any()
                || (pTarget.TryGetAEManager(out AttachEffectScript aem)
                    && aem.TryGetMarks(out HashSet<string> marks)
                    && (Data.OnlyAffectMarks.Intersect(marks).Count() > 0)
                );
        }

    }

}
