using System;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class SHPFVTurretData
    {

        public int WeaponTurretFrameIndex;
        public string WeaponTurretCustomSHP;

        public SHPFVTurretData()
        {
            this.WeaponTurretFrameIndex = -1;
            this.WeaponTurretCustomSHP = null;
        }

        public void Read(ISectionReader reader, int index)
        {
            this.WeaponTurretFrameIndex = reader.Get("WeaponTurretFrameIndex" + index, this.WeaponTurretFrameIndex);
            this.WeaponTurretCustomSHP = reader.Get("WeaponTurretCustomSHP" + index, this.WeaponTurretCustomSHP);
        }

    }

}
