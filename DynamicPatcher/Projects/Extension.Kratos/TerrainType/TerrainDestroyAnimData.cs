using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    [Serializable]
    public class TerrainDestroyAnimData : ExpandAnimsData
    {
        private const string TITLE = "Destroy.";

        public TerrainDestroyAnimData() : base() { }


        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);
        }

    }
}
