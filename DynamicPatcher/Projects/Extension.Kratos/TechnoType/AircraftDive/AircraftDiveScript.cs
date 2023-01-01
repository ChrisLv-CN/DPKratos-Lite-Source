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
        }

        public override void OnUpdate()
        {
            if (!pTechno.IsDeadOrInvisible())
            {
                // 没有目标或处于起飞降落，停留在机场时
                Pointer<FlyLocomotionClass> pFly = pTechno.Convert<FootClass>().Ref.Locomotor.ToLocomotionClass<FlyLocomotionClass>();
                if (!pTechno.InAir()
                    || pFly.Ref.IsTakingOff || pFly.Ref.IsLanding)
                {
                    // 归零
                    DiveStatus = AircraftDiveStatus.NONE;
                    activeDive = false;
                    attitudeScript.UnLock();
                    return;
                }

                // 带蛋起飞，并且高度超过设定值时，开启俯冲
                if (pFly.Ref.IsElevating && pTechno.Ref.Base.GetHeight() >= data.FlightLevel)
                {
                    activeDive = true;
                }

                Pointer<AbstractClass> pTarget = pTechno.Ref.Target;
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 状态 {DiveStatus} pFly {pFly} 战机高度 {pTechno.Ref.Base.GetHeight()}，飞行高度 {pFly.Ref.FlightLevel}， 设定飞行高度 {pTechno.Ref.Type.Ref.FlightLevel}");
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
                                // 持续保持头对准目标
                                attitudeScript.UpdateHeadToCoord(pTarget.Ref.GetCoords(), true);
                            }
                        }
                        break;
                    case AircraftDiveStatus.PULLUP:
                        // 恢复飞行高度
                        DiveStatus = AircraftDiveStatus.NONE;
                        // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 拉起飞机 修改飞行高度为 {pFly.Ref.FlightLevel}");
                        break;
                    default:
                        attitudeScript.UnLock();
                        if (!pTarget.IsNull && activeDive)
                        {
                            // 检查距离目标的距离是否足够近以触发俯冲姿态
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
                                // 进入俯冲状态
                                DiveStatus = AircraftDiveStatus.DIVEING;
                                // 调整飞行高度
                                pFly.Ref.FlightLevel = data.FlightLevel;
                                // 头对准目标
                                attitudeScript.UpdateHeadToCoord(pTarget.Ref.GetCoords(), true);
                                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 开始俯冲 修改飞行高度为 {pFly.Ref.FlightLevel}");
                            }
                        }
                        break;
                }
            }
        }

        public override void OnFire(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            // Logger.Log($"{Game.CurrentFrame} 投弹 是否拉起 {aircraftDiveData.PullUpAfterFire}");
            if (data.PullUpAfterFire && DiveStatus == AircraftDiveStatus.DIVEING)
            {
                DiveStatus = AircraftDiveStatus.PULLUP;
            }
        }
    }
}