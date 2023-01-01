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

        private IConfigWrapper<PassengersData> _passengersData;
        private PassengersData passengersData
        {
            get
            {
                if (null == _passengersData)
                {
                    _passengersData = Ini.GetConfig<PassengersData>(Ini.RulesDependency, section);
                }
                return _passengersData.Data;
            }
        }

        /// <summary>
        /// 乘客只有塞进OpenTopped的载具内，才会执行Update
        /// </summary>
        public void OnUpdate_Passenger()
        {
            if (!isBuilding)
            {
                // check the transporter settings
                Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
                if (!pTransporter.IsNull)
                {
                    // 获取运输载具上设置的乘客行为
                    if (pTransporter.TryGetStatus(out TechnoStatusScript transporter))
                    {
                        PassengersData data = transporter.passengersData;
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

        public bool CanFire_Passenger(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon)
        {
            bool ceaseFire = false;
            if (!isBuilding)
            {
                // check the transporter settings
                Pointer<TechnoClass> pTransporter = pTechno.Ref.Transporter;
                if (!pTransporter.IsNull)
                {
                    if (pTransporter.TryGetStatus(out TechnoStatusScript transporter))
                    {
                        PassengersData data = transporter.passengersData;
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
            return ceaseFire;
        }

    }
}