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
    public class DisguiseData : INIAutoConfig
    {
        public string DefaultDisguise = null; // 步兵

        public string DefaultUnitDisguise = null; // 坦克
    }
}
