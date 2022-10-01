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

        public AttackBeaconState AttackBeaconState = new AttackBeaconState();

        public bool AttackBeaconRecruited = false; // 被征召

        public void OnPut_AttackBeacon()
        {
            // 初始化状态机
            AttackBeaconData data = Ini.GetConfig<AttackBeaconData>(Ini.RulesDependency, section).Data;
            if (data.Enable)
            {
                AttackBeaconState.Enable(data);
            }
        }

        public void OnUpdate_AttackBeacon()
        {
            if (AttackBeaconState.IsReady())
            {
                AttackBeaconState.Reload();
                AttackBeaconData data = AttackBeaconState.Data;

                bool noLimit = null == data.Types || data.Types.Length <= 0;
                // Find self Unit and set ther Target
                Pointer<HouseClass> pHouse = pTechno.Ref.Owner;
                CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                // find candidate
                Dictionary<string, SortedList<double, List<Pointer<TechnoClass>>>> candidates = new Dictionary<string, SortedList<double, List<Pointer<TechnoClass>>>>();
                TechnoClass.Array.FindObject((pTarget) =>
                {
                    string type = pTarget.Ref.Type.Ref.Base.Base.ID;

                    if ((noLimit || data.Types.Contains(type))
                        && (data.Force ? true : AttackBeaconState.RecruitMissions.Contains(pTarget.Ref.Base.GetCurrentMission())))
                    {
                        double distance = location.DistanceFrom(pTarget.Ref.Base.Base.GetCoords());
                        if (distance > data.RangeMin && (data.RangeMax < 0 ? true : distance < data.RangeMax))
                        {
                            if (data.Force || pTarget.Ref.Target.IsNull || pTarget.Ref.Target != pTechno.Convert<AbstractClass>())
                            {
                                // find one
                                SortedList<double, List<Pointer<TechnoClass>>> technoDistanceSorted = null;
                                if (candidates.ContainsKey(type))
                                {
                                    technoDistanceSorted = candidates[type];
                                }
                                else
                                {
                                    technoDistanceSorted = new SortedList<double, List<Pointer<TechnoClass>>>();
                                    candidates.Add(type, technoDistanceSorted);
                                }
                                if (technoDistanceSorted.ContainsKey(distance))
                                {
                                    technoDistanceSorted[distance].Add(pTarget);
                                }
                                else
                                {
                                    List<Pointer<TechnoClass>> recruits = new List<Pointer<TechnoClass>>();
                                    recruits.Add(pTarget);
                                    technoDistanceSorted.Add(distance, recruits);
                                }
                                candidates[type] = technoDistanceSorted;
                            }
                        }
                    }
                    return false;
                }, pHouse, data.AffectsOwner, data.AffectsAllies, data.AffectsEnemies, data.AffectsCivilian);

                Pointer<AbstractClass> pBeacon = pTechno.Convert<AbstractClass>();
                if (data.TargetToCell)
                {
                    Pointer<CellClass> pCell = MapClass.Instance.GetCellAt(pBeacon.Ref.GetCoords());
                    pBeacon = pCell.Convert<AbstractClass>();
                }

                int recruitMax = data.Count > 1 ? data.Count : 99999;
                int recruitCount = 0;
                foreach (var candidate in candidates)
                {
                    string type = candidate.Key;
                    var technos = candidate.Value;
                    // check this type is full.
                    int typeCount = 0;
                    bool isFull = false;
                    foreach (var targets in technos)
                    {
                        if (isFull)
                        {
                            break;
                        }
                        foreach (var pTarget in targets.Value)
                        {
                            if (!noLimit && ++typeCount > AttackBeaconState.Types[type])
                            {
                                isFull = true;
                                break;
                            }
                            // recruit limit
                            if (++recruitCount > recruitMax)
                            {
                                return;
                            }
                            // recruit one
                            pTarget.Ref.SetTarget(pBeacon);
                            if (pTarget.TryGetStatus(out TechnoStatusScript targetStatus))
                            {
                                targetStatus.AttackBeaconRecruited = true;
                            }
                        }
                    }
                }
            }
        }

        public unsafe void OnFire_AttackBeacon_Recruit(Pointer<AbstractClass> pTarget, int weaponIndex)
        {
            // 被征召去攻击信标
            if (AttackBeaconRecruited)
            {
                AttackBeaconRecruited = false;
                // clean recruited target
                pTechno.Ref.SetTarget(Pointer<AbstractClass>.Zero);
            }
        }

    }
}
