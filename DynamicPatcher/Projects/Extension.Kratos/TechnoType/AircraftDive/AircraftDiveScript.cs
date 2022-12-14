using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    public enum AircraftDiveStatus
    {
        NONE = 0, DIVEING = 1, PULLUP = 2
    }

    [Serializable]
    [GlobalScriptable(typeof(TechnoExt))]
    [UpdateAfter(typeof(AircraftAttitudeScript))]
    public class AircraftDiveScript : TechnoScriptable
    {
        public AircraftDiveScript(TechnoExt owner) : base(owner) { }

        public AircraftDiveStatus DiveStatus;

        private AircraftAttitudeScript attitudeScript => GameObject.GetComponent<AircraftAttitudeScript>();
        private IConfigWrapper<AircraftDiveData> _data;
        private AircraftDiveData data
        {
            get
            {
                if (null == _data)
                {
                    _data = Ini.GetConfig<AircraftDiveData>(Ini.RulesDependency, section);
                }
                return _data.Data;
            }
        }

        private bool activeDive;

        public override void Awake()
        {
            ILocomotion locomotion = null;
            if (!data.Enable || !pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out Pointer<AircraftClass> pAircraft)
                || (locomotion = pAircraft.Convert<FootClass>().Ref.Locomotor).ToLocomotionClass().Ref.GetClassID() != LocomotionClass.Fly)
            {
                GameObject.RemoveComponent(this);
                return;
            }
            // Pointer<FlyLocomotionClass> pFly = locomotion.ToLocomotionClass<FlyLocomotionClass>();
            // this.flightLevel = pFly.Ref.FlightLevel;
        
            EventSystem.Techno.AddTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public override void OnUnInit()
        {
            EventSystem.Techno.RemoveTemporaryHandler(EventSystem.Techno.TypeChangeEvent, OnTransform);
        }

        public void OnTransform(object sender, EventArgs args)
        {
            Pointer<TechnoClass> pTarget = ((TechnoTypeChangeEventArgs)args).pTechno;
            if (!pTarget.IsNull && pTarget == pTechno)
            {
                _data = null;
            }
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                // ??????????????????????????????????????????????????????
                Pointer<FlyLocomotionClass> pFly = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>();
                if (!pTechno.InAir()
                    || pFly.Ref.IsTakingOff || pFly.Ref.IsLanding)
                {
                    // ??????
                    DiveStatus = AircraftDiveStatus.NONE;
                    activeDive = false;
                    attitudeScript.UnLock();
                    return;
                }

                // ????????????????????????????????????????????????????????????
                if (pFly.Ref.IsElevating && pTechno.Ref.Base.GetHeight() >= data.FlightLevel)
                {
                    activeDive = true;
                }

                Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} ?????? {DiveStatus} pFly {pFly} ???????????? {pTechno.Ref.Base.GetHeight()}??????????????? {pFly.Ref.FlightLevel}??? ?????????????????? {pTechno.Ref.Type.Ref.FlightLevel}");
                switch (DiveStatus)
                {
                    case AircraftDiveStatus.DIVEING:
                        if (pTarget.IsNull)
                        {
                            DiveStatus = AircraftDiveStatus.PULLUP;
                        }
                        else
                        {
                            if (data.PullUpAfterFire)
                            {
                                // ???????????????????????????
                                attitudeScript.UpdateHeadToCoord(pTarget.Ref.GetCoords(), true);
                            }
                        }
                        break;
                    case AircraftDiveStatus.PULLUP:
                        // ??????????????????
                        DiveStatus = AircraftDiveStatus.NONE;
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} ???????????? ????????????????????? {pFly.Ref.FlightLevel}");
                        break;
                    default:
                        attitudeScript.UnLock();
                        if (!pTarget.IsNull && activeDive)
                        {
                            // ???????????????????????????????????????????????????????????????
                            CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                            CoordStruct targetPos = pTarget.Ref.GetCoords();
                            int distance = (int)(data.Distance * 256);
                            if (distance == 0)
                            {
                                int weaponIndex = pTechno.Ref.SelectWeapon(pTarget);
                                distance = pTechno.Ref.GetWeapon(weaponIndex).Ref.WeaponType.Ref.Range * 2;
                            }
                            double dist = 0;
                            if ((dist = location.DistanceFrom(targetPos)) <= distance)
                            {
                                // ??????????????????
                                DiveStatus = AircraftDiveStatus.DIVEING;
                                // ??????????????????
                                pFly.Ref.FlightLevel = data.FlightLevel;
                                // ???????????????
                                attitudeScript.UpdateHeadToCoord(pTarget.Ref.GetCoords(), true);
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} ???????????? ????????????????????? {pFly.Ref.FlightLevel}");
                            }
                        }
                        break;
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            // Logger.Log($"{Game.CurrentFrame} ?????? ???????????? {aircraftDiveData.PullUpAfterFire}");
            if (data.PullUpAfterFire && DiveStatus == AircraftDiveStatus.DIVEING)
            {
                DiveStatus = AircraftDiveStatus.PULLUP;
            }
        }
    }
}