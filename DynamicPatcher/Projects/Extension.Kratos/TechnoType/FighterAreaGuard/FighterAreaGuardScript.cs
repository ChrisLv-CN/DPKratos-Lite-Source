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
    public class FighterAreaGuardScript : TechnoScriptable
    {
        public FighterAreaGuardScript(TechnoExt owner) : base(owner) { }

        private FighterAreaGuardData data => Ini.GetConfig<FighterAreaGuardData>(Ini.RulesDependency, section).Data;

        private bool isAreaProtecting = false;

        private CoordStruct areaProtectTo;

        private static List<CoordStruct> areaGuardCoords = new List<CoordStruct>()
        {
            new CoordStruct(-300,-300,0),
            new CoordStruct(-300,0,0),
            new CoordStruct(0,0,0),
            new CoordStruct(300,0,0),
            new CoordStruct(300,300,0),
            new CoordStruct(0,300,0),
        };

        private int currentAreaProtectedIndex = 0;

        private bool isAreaGuardReloading = false;

        private int areaGuardTargetCheckRof = 20;


        public override void Awake()
        {
            if (!data.AreaGuard || pTechno.Ref.Type.Ref.MissileSpawn || !pTechno.CastToAircraft(out Pointer<AircraftClass> pAircraft))
            {
                GameObject.RemoveComponent(this);
                return;
            }
        }

        public override void OnUpdate()
        {
            if (pTechno.IsDeadOrInvisible() || !data.AreaGuard)
            {
                return;
            }

            var mission = pTechno.Convert<MissionClass>();

            if (mission.Ref.CurrentMission == Mission.Move)
            {
                isAreaProtecting = false;
                isAreaGuardReloading = false;
                return;
            }

            if (pTechno.CastToFoot(out Pointer<FootClass> pfoot))
            {
                if (!isAreaProtecting)
                {
                    if (mission.Ref.CurrentMission == Mission.Area_Guard)
                    {
                        isAreaProtecting = true;

                        CoordStruct dest = pfoot.Ref.Locomotor.Destination();
                        areaProtectTo = dest;
                    }
                }


                if (isAreaProtecting)
                {
                    //没弹药的情况下返回机场
                    if (pTechno.Ref.Ammo == 0 && !isAreaGuardReloading)
                    {
                        pTechno.Ref.SetTarget(default);
                        pTechno.Ref.SetDestination(default(Pointer<CellClass>), false);
                        mission.Ref.ForceMission(Mission.Stop);
                        isAreaGuardReloading = true;
                        return;
                    }

                    //填弹完毕后继续巡航
                    if (isAreaGuardReloading)
                    {
                        if (pTechno.Ref.Ammo >= data.MaxAmmo)
                        {
                            isAreaGuardReloading = false;
                            mission.Ref.ForceMission(Mission.Area_Guard);
                        }
                        else
                        {
                            if (mission.Ref.CurrentMission != Mission.Sleep && mission.Ref.CurrentMission != Mission.Enter)
                            {
                                if (mission.Ref.CurrentMission == Mission.Guard)
                                {
                                    mission.Ref.ForceMission(Mission.Sleep);
                                }
                                else
                                {
                                    mission.Ref.ForceMission(Mission.Enter);
                                }
                                return;
                            }
                        }
                    }


                    if (mission.Ref.CurrentMission == Mission.Move)
                    {
                        isAreaProtecting = false;
                        return;
                    }
                    else if (mission.Ref.CurrentMission == Mission.Attack)
                    {
                        return;
                    }
                    else if (mission.Ref.CurrentMission == Mission.Enter)
                    {
                        if (isAreaGuardReloading)
                        {
                            return;
                        }
                        else
                        {
                            mission.Ref.ForceMission(Mission.Stop);
                        }
                    }
                    else if (mission.Ref.CurrentMission == Mission.Sleep)
                    {
                        if (isAreaGuardReloading)
                        {
                            return;
                        }
                    }


                    if (areaProtectTo != null)
                    {
                        var dest = areaProtectTo;

                        var house = pTechno.Ref.Owner;

                        if (data.AutoFire)
                        {
                            if (areaProtectTo.DistanceFrom(pTechno.Ref.Base.Base.GetCoords()) <= 2000)
                            {
                                if (areaGuardTargetCheckRof-- <= 0)
                                {
                                    areaGuardTargetCheckRof = 20;

                                    Pointer<TechnoClass> pTargetTechno = IntPtr.Zero;
                                    TechnoClass.Array.FindObject((pTarget) =>
                                    {
                                        if (!pTarget.IsDeadOrInvisible())
                                        {
                                            CoordStruct targetPos = pTarget.Ref.Base.Base.GetCoords();
                                            int height = pTarget.Ref.Base.GetHeight();
                                            AbstractType type = pTarget.Ref.Base.Base.WhatAmI();
                                            int bounsRange = 0;
                                            if (height > 10)
                                            {
                                                bounsRange = data.GuardRange;
                                            }
                                            if ((targetPos - new CoordStruct(0, 0, height)).DistanceFrom(dest) <= (data.GuardRange * 256 + bounsRange) && type != AbstractType.Building)
                                            {
                                                pTargetTechno = pTarget;
                                                return true;
                                            }
                                        }
                                        return false;
                                    }, house, false, false, true, false);
                                    if (!pTargetTechno.IsDeadOrInvisible())
                                    {
                                        pTechno.Ref.SetTarget(pTargetTechno.Convert<AbstractClass>());
                                        mission.Ref.ForceMission(Mission.Stop);
                                        mission.Ref.ForceMission(Mission.Attack);
                                        return;
                                    }
                                }

                            }
                        }



                        if (areaProtectTo.DistanceFrom(pTechno.Ref.Base.Base.GetCoords()) <= 2000)
                        {
                            if (currentAreaProtectedIndex > areaGuardCoords.Count() - 1)
                            {
                                currentAreaProtectedIndex = 0;
                            }
                            dest += areaGuardCoords[currentAreaProtectedIndex];
                            currentAreaProtectedIndex++;
                        }

                        pfoot.Ref.Locomotor.Move_To(dest);
                        var cell = MapClass.Coord2Cell(dest);
                        if (MapClass.Instance.TryGetCellAt(cell, out Pointer<CellClass> pcell))
                        {
                            pTechno.Ref.SetDestination(pcell, false);
                        }
                    }
                }
            }
        }
    }
}