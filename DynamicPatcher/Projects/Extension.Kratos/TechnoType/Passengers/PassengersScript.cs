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


    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(TechnoStatusScript))]
    public class PassengersScript : TechnoScriptable
    {
        public PassengersScript(TechnoExt owner) : base(owner) { }

        private PassengersData data => Ini.GetConfig<PassengersData>(Ini.RulesDependency, section).Data;
        private UploadAttachTypeData loadTypeData => Ini.GetConfig<UploadAttachTypeData>(Ini.RulesDependency, section).Data;

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                // check the transporter settings
                Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
                if (!pTransporter.IsNull)
                {
                    // 获取运输载具上设置的乘客行为
                    if (pTransporter.TryGetComponent<PassengersScript>(out PassengersScript transporter))
                    {
                        PassengersData data = transporter.data;
                        if (null != data && data.OpenTopped)
                        {
                            if (!data.PassiveAcquire)
                            {
                                Mission transporterMission = pTransporter.Convert<ObjectClass>().Ref.GetCurrentMission();
                                // Mission mission = pTechno.Convert<ObjectClass>().Ref.GetCurrentMission();
                                if (transporterMission != Mission.Attack)
                                // if (mission != Mission.Attack && mission != Mission.Sleep)
                                {
                                    pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Sleep, true);
                                }
                            }
                            if (data.ForceFire)
                            {
                                pTechno.Ref.SetTarget(pTransporter.Ref.Target);
                            }
                        }
                    }
                    // 为运输载具附加AE
                    if (loadTypeData.Enable && pTransporter.TryGetAEManager(out AttachEffectScript aeManager))
                    {
                        foreach (UploadAttachData loadData in loadTypeData.Datas.Values)
                        {
                            if (loadData.Enable
                                && loadData.CanAffectType(pTransporter)
                                && (loadData.AffectInAir || !pTransporter.InAir())
                                && (loadData.AffectStand || !pTransporter.AmIStand())
                                && loadData.IsOnMark(aeManager)
                            )
                            {
                                aeManager.Attach(loadData.AttachEffects);
                            }
                        }
                    }
                }
            }
        }

        public override void CanFire(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon, ref bool ceaseFire)
        {
            if (!ceaseFire)
            {
                // check the transporter settings
                Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
                if (!pTransporter.IsNull)
                {
                    if (pTransporter.TryGetComponent<PassengersScript>(out PassengersScript transporter))
                    {
                        PassengersData data = transporter.data;
                        if (null != data && data.OpenTopped)
                        {
                            Mission transporterMission = pTransporter.Convert<ObjectClass>().Ref.GetCurrentMission();
                            switch (transporterMission)
                            {
                                case Mission.Attack:
                                    ceaseFire = !data.SameFire;
                                    break;
                                case Mission.Move:
                                case Mission.AttackMove:
                                    ceaseFire = !data.MobileFire;
                                    break;
                            }
                        }
                    }
                }
            }
        }

    }
}