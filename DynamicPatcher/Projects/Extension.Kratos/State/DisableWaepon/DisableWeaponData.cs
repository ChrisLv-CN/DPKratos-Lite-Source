using System;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public DisableWeaponData DisableWeaponData;

        private void ReadDisableWeaponData(IConfigReader reader)
        {
            DisableWeaponData data = new DisableWeaponData();
            data.Read(reader);
            if (data.Enable)
            {
                this.DisableWeaponData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class DisableWeaponData : EffectData, IStateData
    {
        public const string TITLE = "Weapon.";

        public bool Disable;

        public LandType[] OnLandTypes;

        public DisableWeaponData()
        {
            this.Disable = false;
            this.OnLandTypes = null;

            this.AffectWho = AffectWho.ALL;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Disable = reader.Get(TITLE + "Disable", false);
            this.Enable = this.Disable;

            this.OnLandTypes = reader.GetList<LandType>(TITLE + "DisableOnLands", this.OnLandTypes);
        }

    }


}
