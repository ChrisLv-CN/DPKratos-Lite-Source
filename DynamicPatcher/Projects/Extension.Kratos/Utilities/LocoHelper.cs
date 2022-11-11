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
            Guid id = loco.ToLocomotionClass().Ref.GetClassID();
            if (id == LocomotionClass.Drive)
            {
                Pointer<DriveLocomotionClass> pLoco = loco.ToLocomotionClass<DriveLocomotionClass>();
                pLoco.Ref.IsDriving = false;
            }
            else if (id == LocomotionClass.Ship)
            {
                Pointer<ShipLocomotionClass> pLoco = loco.ToLocomotionClass<ShipLocomotionClass>();
                pLoco.Ref.IsDriving = false;
            }
        }

    }
}
