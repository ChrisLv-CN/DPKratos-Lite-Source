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
    public class SpawnFireOnceData : INIAutoConfig
    {
        [INIField(Key = "SpawnFireOnce")]
        public bool FireOnce = false;

        [INIField(Key = "SpawnFireOnceDelay")]
        public int Delay = 0;

    }

}
