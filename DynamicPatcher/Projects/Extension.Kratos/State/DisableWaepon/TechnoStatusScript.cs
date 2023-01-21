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

        public State<DisableWeaponData> DisableWeaponState = new State<DisableWeaponData>();

        public bool CanFire_DisableWeapon(Pointer<AbstractClass> pTarget, Pointer<WeaponTypeClass> pWeapon)
        {
            if (DisableWeaponState.IsActive())
            {
                DisableWeaponData data = DisableWeaponState.Data;
                if (null != data.OnLandTypes && data.OnLandTypes.Length > 0)
                {
                    CoordStruct location = pTechno.Ref.Base.Base.GetCoords();
                    if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
                    {
                        LandType landType = pCell.Ref.LandType;
                        if (landType == LandType.Water && pCell.Ref.Flags.HasFlag(CellFlags.Bridge))
                        {
                            // 将水面上的桥强制判定为路面
                            landType = LandType.Road;
                        }
                        // Logger.Log("当前格子的地形类型{0}, 瓷砖类型{1}", landType, pCell.Ref.GetTileType());
                        if (data.OnLandTypes.Contains(landType))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                return true;
            }
            return false;
        }

    }
}
