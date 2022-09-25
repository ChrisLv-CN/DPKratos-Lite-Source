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

    public class DestroyAnimsData : INIAutoConfig
    {
        [INIField(Key = "DestroyAnims")]
        public string[] DestroyAnims = null;

        [INIField(Key = "DestroyAnims.Random")]
        public bool DestroyAnimsRandom = true;

        public DestroyAnimsData()
        {
            this.DestroyAnims = null;
            this.DestroyAnimsRandom = true;
        }

    }


}
