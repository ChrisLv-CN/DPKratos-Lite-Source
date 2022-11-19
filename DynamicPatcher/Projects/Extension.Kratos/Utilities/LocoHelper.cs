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

namespace Extension.Utilities
{

    public static class LocoHelper
    {

        public static unsafe void ForceStopMoving(this ILocomotion loco)
        {
            loco.Stop_Moving();
            loco.Mark_All_Occupation_Bits(0);
            Guid id = loco.ToLocomotionClass().Ref.GetClassID();
            if (id == LocomotionClass.Drive)
            {
                Pointer<DriveLocomotionClass> pLoco = loco.ToLocomotionClass<DriveLocomotionClass>();
                pLoco.Ref.Destination = default;
                pLoco.Ref.HeadToCoord = default;
                pLoco.Ref.IsDriving = false;
            }
            else if (id == LocomotionClass.Ship)
            {
                Pointer<ShipLocomotionClass> pLoco = loco.ToLocomotionClass<ShipLocomotionClass>();
                pLoco.Ref.Destination = default;
                pLoco.Ref.HeadToCoord = default;
                pLoco.Ref.IsDriving = false;
            }
            else if (id == LocomotionClass.Walk)
            {
                Pointer<WalkLocomotionClass> pLoco = loco.ToLocomotionClass<WalkLocomotionClass>();
                pLoco.Ref.Destination = default;
                pLoco.Ref.HeadToCoord = default;
                pLoco.Ref.IsMoving = false;
                pLoco.Ref.IsReallyMoving = false;
            }
            else if (id == LocomotionClass.Mech)
            {
                Pointer<MechLocomotionClass> pLoco = loco.ToLocomotionClass<MechLocomotionClass>();
                pLoco.Ref.Destination = default;
                pLoco.Ref.HeadToCoord = default;
                pLoco.Ref.IsMoving = false;
            }
        }

    }
}
