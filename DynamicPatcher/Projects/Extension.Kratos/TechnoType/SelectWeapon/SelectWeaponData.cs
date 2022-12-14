using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class SelectWeaponData : INIConfig
    {
        public bool UseRange;

        public SelectWeaponData()
        {
            UseRange = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.UseRange = reader.Get("SelectWeaponUseRange", this.UseRange);
        }

    }


}
