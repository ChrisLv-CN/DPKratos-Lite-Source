using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Utilities
{

    public static class WarheadTypeHelper
    {

        public static WarheadTypeData GetData(this Pointer<WarheadTypeClass> pWH)
        {
            return pWH.GetData(out WarheadTypeExt ext);
        }

        public static WarheadTypeData GetData(this Pointer<WarheadTypeClass> pWH, out WarheadTypeExt whExt)
        {
            whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return whData.Data;
            }
            return new WarheadTypeData();
        }

        public static AttachEffectTypeData GetAEData(this Pointer<WarheadTypeClass> pWH)
        {
            return pWH.GetAEData(out WarheadTypeExt ext);
        }

        public static AttachEffectTypeData GetAEData(this Pointer<WarheadTypeClass> pWH, out WarheadTypeExt whExt)
        {
            whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<AttachEffectTypeData> aeData = (IConfigWrapper<AttachEffectTypeData>)whExt.WarheadAEData;
                if (null == aeData)
                {
                    aeData = Ini.GetConfig<AttachEffectTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadAEData = (INIComponent)aeData;
                }
                return aeData.Data;
            }
            return new AttachEffectTypeData();
        }

        public static Pointer<AnimClass> PlayWarheadAnim(this Pointer<WarheadTypeClass> pWH, CoordStruct location, int damage = 1, LandType landType = LandType.Clear)
        {
            Pointer<AnimClass> pAnim = IntPtr.Zero;
            if (MapClass.Instance.TryGetCellAt(location, out Pointer<CellClass> pCell))
            {
                landType = pCell.Ref.LandType;
            }
            Pointer<AnimTypeClass> pAnimType = MapClass.SelectDamageAnimation(damage, pWH, landType, location);
            if (!pAnimType.IsNull)
            {
                pAnim = YRMemory.Create<AnimClass>(pAnimType, location);
            }
            return pAnim;
        }

        public static bool HasPreImpactAnim(this Pointer<WarheadTypeClass> pWH)
        {
            if (!pWH.IsNull)
            {
                WarheadTypeData whData = pWH.GetData();
                return !whData.PreImpactAnim.IsNullOrEmptyOrNone();
            }
            return false;
        }

        public static bool CanAffectHouse(this Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pOwnerHouse, Pointer<HouseClass> pTargetHouse)
        {
            return CanAffectHouse(pWH, pOwnerHouse, pTargetHouse, out WarheadTypeData data);
        }

        public static bool CanAffectHouse(this Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pOwnerHouse, Pointer<HouseClass> pTargetHouse, out WarheadTypeData whData)
        {
            whData = pWH.GetData();
            return pWH.CanAffectHouse(pOwnerHouse, pTargetHouse, whData);
        }

        public static bool CanAffectHouse(this Pointer<WarheadTypeClass> pWH, Pointer<HouseClass> pOwnerHouse, Pointer<HouseClass> pTargetHouse, WarheadTypeData whData)
        {
            if (!pOwnerHouse.IsNull && !pTargetHouse.IsNull)
            {
                if (pOwnerHouse == pTargetHouse)
                {
                    return whData.AffectsAllies || whData.AffectsOwner;
                }
                else if (pOwnerHouse.Ref.IsAlliedWith(pTargetHouse))
                {
                    return whData.AffectsAllies;
                }
                else
                {
                    return whData.AffectsEnemies;
                }
            }
            return true;
        }

        public static bool CanReaction(this Pointer<WarheadTypeClass> pWH, out DamageReactionMode[] ignoreModes)
        {
            ignoreModes = null;
            WarheadTypeData data = pWH.GetData(out WarheadTypeExt whExt);
            if (null != whExt)
            {
                ignoreModes = data.IgnoreDamageReactionModes;
                return !data.IgnoreDamageReaction || (null != ignoreModes && ignoreModes.Any());
            }
            return true;
        }

    }
}
