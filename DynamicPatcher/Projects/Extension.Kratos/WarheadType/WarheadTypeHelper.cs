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
                WarheadTypeData whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
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
            whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
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

        public static bool IsToy(this Pointer<WarheadTypeClass> pWH)
        {
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return whData.Data.IsToy;
            }
            return false;
        }

        public static double GetVersus(this Pointer<WarheadTypeClass> pWH, Armor armor)
        {
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return whData.Data.GetVersus(armor);
            }
            return 1d;
        }

        public static bool IsTeleporter(this Pointer<WarheadTypeClass> pWH)
        {
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return whData.Data.Teleporter;
            }
            return false;
        }

        public static bool IsCapturer(this Pointer<WarheadTypeClass> pWH)
        {
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return whData.Data.Capturer;
            }
            return false;
        }

        public static bool CanRevenge(this Pointer<WarheadTypeClass> pWH)
        {
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return !whData.Data.IgnoreRevenge;
            }
            return false;
        }

        public static bool CanReaction(this Pointer<WarheadTypeClass> pWH, out DamageReactionMode[] ignoreModes)
        {
            // WarheadTypeData whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            // ignoreModes = whData.IgnoreDamageReactionModes;
            // return !whData.IgnoreDamageReaction || (null != ignoreModes && ignoreModes.Any());
            ignoreModes = null;
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                ignoreModes = whData.Data.IgnoreDamageReactionModes;
                return !whData.Data.IgnoreDamageReaction || (null != ignoreModes && ignoreModes.Any());
            }
            return true;
        }

        public static bool CanShareDamage(this Pointer<WarheadTypeClass> pWH)
        {
            // WarheadTypeData whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID).Data;
            // return !whData.IgnoreStandShareDamage;
            WarheadTypeExt whExt = WarheadTypeExt.ExtMap.Find(pWH);
            if (null != whExt)
            {
                IConfigWrapper<WarheadTypeData> whData = (IConfigWrapper<WarheadTypeData>)whExt.WarheadTypeData;
                if (null == whData)
                {
                    whData = Ini.GetConfig<WarheadTypeData>(Ini.RulesDependency, pWH.Ref.Base.ID);
                    whExt.WarheadTypeData = (INIComponent)whData;
                }
                return !whData.Data.IgnoreStandShareDamage;
            }
            return true;
        }

    }
}
