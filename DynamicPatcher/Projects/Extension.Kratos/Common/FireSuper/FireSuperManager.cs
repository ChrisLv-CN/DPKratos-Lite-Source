using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.EventSystems;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class FireSuperManager
    {
        static FireSuperManager()
        {
            EventSystem.General.AddPermanentHandler(EventSystem.General.LogicClassUpdateEvent, FireSuperManager.Update);
        }

        private static Queue<FireSuperWeapon> fireSuperWeaponQueue = new Queue<FireSuperWeapon>();

        public static void Order(Pointer<HouseClass> pHouse, CoordStruct location, FireSuperEntity data)
        {
            CellStruct cellStruct = MapClass.Coord2Cell(location);
            HouseExt houseExt = !pHouse.IsNull ? HouseExt.ExtMap.Find(pHouse) : HouseExt.ExtMap.Find(HouseClass.FindSpecial());
            FireSuperWeapon fireSuperWeapon = new FireSuperWeapon(houseExt, cellStruct, data);
            fireSuperWeaponQueue.Enqueue(fireSuperWeapon);
        }

        public static void Launch(Pointer<HouseClass> pHouse, CoordStruct location, FireSuperEntity data)
        {
            CellStruct cellStruct = MapClass.Coord2Cell(location);
            LaunchSuperWeapons(pHouse, cellStruct, data);
        }

        public static void Reset()
        {
            fireSuperWeaponQueue.Clear();
        }

        public static void Update(object sender, EventArgs args)
        {
            for (int i = 0; i < fireSuperWeaponQueue.Count; i++)
            {
                FireSuperWeapon fireSuperWeapon = fireSuperWeaponQueue.Dequeue();
                if (fireSuperWeapon.CanLaunch())
                {
                    LaunchSuperWeapons(fireSuperWeapon.House, fireSuperWeapon.Location, fireSuperWeapon.Data);
                    fireSuperWeapon.Cooldown();
                }
                if (!fireSuperWeapon.IsDone())
                {
                    fireSuperWeaponQueue.Enqueue(fireSuperWeapon);
                }
            }
        }

        private static void LaunchSuperWeapons(Pointer<HouseClass> pHouse, CellStruct targetPos, FireSuperEntity data)
        {
            if (null != data)
            {
                // Check House alive
                if (pHouse.IsNull || pHouse.Ref.Defeated)
                {
                    // find civilian
                    pHouse = HouseClass.FindCivilianSide();
                    if (pHouse.IsNull)
                    {
                        Logger.LogWarning("Want to fire a super weapon {0}, but house is null.", data.Supers.ToArray());
                        return;
                    }
                }
                int superCount = data.Supers.Length;
                int chanceCount = null != data.Chances ? data.Chances.Length : 0;
                for (int index = 0; index < superCount; index++)
                {
                    // 检查概率
                    if (data.Chances.Bingo(index))
                    {
                        string superID = data.Supers[index];
                        Pointer<SuperWeaponTypeClass> pType = SuperWeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(superID);
                        if (!pType.IsNull)
                        {
                            Pointer<SuperClass> pSuper = pHouse.Ref.FindSuperWeapon(pType);
                            if (pSuper.Ref.IsCharged || !data.RealLaunch)
                            {
                                pSuper.Ref.IsCharged = true;
                                pSuper.Ref.Launch(targetPos, true);
                                pSuper.Ref.IsCharged = false;
                                pSuper.Ref.Reset();
                            }
                        }
                    }

                }

            }
        }

    }

}
