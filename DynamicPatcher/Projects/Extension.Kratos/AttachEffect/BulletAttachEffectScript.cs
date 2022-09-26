using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    [GlobalScriptable(typeof(BulletExt))]
    [UpdateAfter(typeof(BulletStatusScript))]
    public class BulletAttachEffectScript : BulletScriptable
    {
        public BulletAttachEffectScript(BulletExt owner) : base(owner) { }

        private BulletStatusScript bulletStatus => GameObject.GetComponent<BulletStatusScript>();

    }
}
