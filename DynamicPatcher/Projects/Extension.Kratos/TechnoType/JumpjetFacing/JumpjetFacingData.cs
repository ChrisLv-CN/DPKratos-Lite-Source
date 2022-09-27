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
    public class JumpjetFacingData : INIConfig
    {
        public const string TITLE = "DamageText.";

        public bool Enable;
        public int Facing;

        public JumpjetFacingData()
        {
            this.Enable = false;
            this.Facing = 8;
        }


        public override void Read(IConfigReader reader)
        {
            this.Enable = reader.Get("JumpjetFacingToTarget", this.Enable);
            this.Facing = reader.Get("JumpjetFacing", this.Facing);
        }
    }



}