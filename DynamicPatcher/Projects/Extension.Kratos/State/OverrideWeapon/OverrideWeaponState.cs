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

namespace Extension.Ext
{

    public class OverrideWeaponState : State<OverrideWeaponData>
    {

        public int WeaponIndex = -1;

        public bool TryGetOverrideWeapon(bool isElite, out Pointer<WeaponTypeClass> pOverrideWeapon)
        {
            pOverrideWeapon = IntPtr.Zero;
            if (IsActive() && CanOverride(isElite, out string weaponType))
            {
                pOverrideWeapon = WeaponTypeClass.ABSTRACTTYPE_ARRAY.Find(weaponType);
                return !pOverrideWeapon.IsNull;
            }
            return false;
        }

        private bool CanOverride(bool isElite, out string weaponType)
        {
            weaponType = null;
            if (null != Data && WeaponIndex >= 0)
            {
                OverrideWeapon data = Data.Data;
                if (isElite)
                {
                    data = Data.EliteData;
                }
                if (null != data)
                {
                    string[] types = data.Types;
                    bool isRandomType = data.RandomType;
                    int[] weights = data.Weights;
                    int overrideIndex = data.Index;
                    double chance = data.Chance;
                    weaponType = types[0];
                    if (isRandomType)
                    {
                        // 算权重
                        int typeCount = types.Length;
                        // 获取权重标靶
                        Dictionary<Point2D, int> targetPad = weights.MakeTargetPad(typeCount, out int maxValue);
                        // 中
                        int i = targetPad.Hit(maxValue);
                        weaponType = types[i];
                    }
                    if (!string.IsNullOrEmpty(weaponType) && (overrideIndex < 0 || overrideIndex == WeaponIndex))
                    {
                        // 算概率
                        return chance >= 1 || chance >= MathEx.Random.NextDouble();
                    }
                }
            }
            return false;
        }
    }

}
