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

        private PassengersData _data;
        private PassengersData data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<PassengersData>(Ini.RulesDependency, section).Data;
                }
                return _data;
            }
        }

        public override void Awake()
        {
            if (!pTechno.Convert<AbstractClass>().Ref.AbstractFlags.HasFlag(AbstractFlags.Foot))
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        /// <summary>
        /// 乘客只有塞进OpenTopped的载具内，才会执行Update
        /// </summary>
        public override void OnUpdate()
        {
            if (!pTechno.IsDead())
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