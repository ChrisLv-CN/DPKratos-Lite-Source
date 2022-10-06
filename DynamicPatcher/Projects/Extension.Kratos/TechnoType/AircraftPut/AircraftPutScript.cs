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
    public class AircraftPutScript : TechnoScriptable
    {
        public AircraftPutScript(TechnoExt owner) : base(owner) { }

        private AircraftPutData aircraftPutData => Ini.GetConfig<AircraftPutData>(Ini.RulesDependency, section).Data;

        private bool aircraftPutOffsetFlag = false;
        private bool aircraftPutOffset = false;

        public override void Awake()
        {
            if (!pTechno.CastIf<AircraftClass>(AbstractType.Aircraft, out Pointer<AircraftClass> pAircraft)
                || null == aircraftPutData.PadAircraftTypes || !aircraftPutData.PadAircraftTypes.Contains(section))
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnPut(Pointer<CoordStruct> pLocation, DirType dirType)
        {
            // 停机坪满了移除飞机
            if (pTechno.Convert<AircraftClass>().Ref.Type.Ref.AirportBound && aircraftPutData.RemoveIfNoDocks)
            {
                if (pTechno.Ref.Owner.Ref.AirportDocks <= 0 || pTechno.Ref.Owner.Ref.AirportDocks < CountAircraft(aircraftPutData.PadAircraftTypes))
                {
                    int cost = pTechno.Ref.Type.Ref.Cost;
                    if (cost > 0)
                    {
                        pTechno.Ref.Owner.Ref.GiveMoney(cost);
                    }
                    else
                    {
                        pTechno.Ref.Owner.Ref.TakeMoney(-cost);
                    }
                    pTechno.Ref.Base.Remove();
                    pTechno.Ref.Base.UnInit();
                    return;
                }
            }

            // 调整飞机出生点位
            if (!aircraftPutOffsetFlag && default != aircraftPutData.NoHelipadPutOffset)
            {
                aircraftPutOffsetFlag = true;
                aircraftPutOffset = true;
                if (!aircraftPutData.ForcePutOffset)
                {
                    // check Building has Helipad
                    if (MapClass.Instance.TryGetCellAt(pLocation.Ref, out Pointer<CellClass> pCell))
                    {
                        Pointer<BuildingClass> pBuilding = pCell.Ref.GetBuilding();
                        if (!pBuilding.IsNull && pBuilding.Ref.Type.Ref.Helipad)
                        {
                            aircraftPutOffset = false;
                        }
                    }
                }
                if (aircraftPutOffset)
                {
                    pLocation.Ref += aircraftPutData.NoHelipadPutOffset;
                }
            }
        }

        public override void OnUpdate()
        {
            if (aircraftPutOffset)
            {
                aircraftPutOffset = false;
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                CoordStruct pos = location + aircraftPutData.NoHelipadPutOffset;
                // Logger.Log("Change put Location {0} to {1}", location, pos);
                pTechno.Ref.Base.SetLocation(pos);
                if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                {
                    pTechno.Ref.SetDestination(pCell, true);
                }
                pTechno.Convert<MissionClass>().Ref.QueueMission(Mission.Enter, false);
            }
        }

        private int CountAircraft(String[] padList)
        {
            int count = 0;
            AircraftClass.Array.FindObject((pTarget) =>
            {
                if (padList.Contains(pTarget.Ref.Type.Ref.Base.Base.Base.ID)
                    && pTarget.Ref.Type.Ref.AirportBound)
                {
                    count++;
                }
                return false;
            }, default, 0, pTechno.Ref.Owner);
            return count;
        }

    }
}