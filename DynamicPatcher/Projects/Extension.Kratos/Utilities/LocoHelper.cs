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

        public static unsafe void ForceStopMoving(this Pointer<FootClass> pFoot)
        {
            // 清除移动目的地
            pFoot.Ref.Base.Focus = default;
            pFoot.Ref.Base.SetDestination(default(Pointer<CellClass>));
            pFoot.Ref.Destination = default;
            pFoot.Ref.LastDestination = default;
            // 清除寻路目的地
            // LocomotionClass.ChangeLocomotorTo(pFoot, LocomotionClass.Jumpjet);
            ILocomotion loco = pFoot.Ref.Locomotor;
            loco.Mark_All_Occupation_Bits((int)MarkType.UP); // 清除HeadTo的占领
            if (loco.Apparent_Speed() > 0)
            {
                // Logger.Log($"{Game.CurrentFrame} [{section}]{pTechno} 受黑洞 [{pBlackHole.Ref.Type.Ref.Base.ID}] {pBlackHole.Pointer} 的影响 speed={loco.Apparent_Speed()} IsMoving={loco.Is_Moving()} IsMovingNow={loco.Is_Moving_Now()} IsReallyMovingNow={loco.Is_Really_Moving_Now()}");
                loco.ForceStopMoving();
            }
        }

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
