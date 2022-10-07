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
    public enum PlaySuperWeaponMode
    {
        CUSTOM = 0, DONE = 1, LOOP = 2
    }
    public class PlaySuperWeaponModeParser : KEnumParser<PlaySuperWeaponMode>
    {
        public override bool ParseInitials(string t, ref PlaySuperWeaponMode buffer)
        {
            switch (t)
            {
                case "D":
                    buffer = PlaySuperWeaponMode.DONE;
                    return true;
                case "L":
                    buffer = PlaySuperWeaponMode.LOOP;
                    return true;
                case "C":
                    buffer = PlaySuperWeaponMode.CUSTOM;
                    return true;
            }
            return false;
        }
    }

    [Serializable]
    public class PlaySuperData : FireSuperData
    {
        public PlaySuperWeaponMode LaunchMode;

        static PlaySuperData()
        {
            new PlaySuperWeaponModeParser().Register();
        }

        public PlaySuperData()
        {
            this.LaunchMode = PlaySuperWeaponMode.DONE;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader);

            this.LaunchMode = reader.Get(TITLE + "LaunchMode", this.LaunchMode);
            this.Enable = null != Data;
        }

    }


}
