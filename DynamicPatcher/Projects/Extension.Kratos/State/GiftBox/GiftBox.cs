using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class AttachEffect
    {
        public GiftBox GiftBox;

        private void InitGiftBox()
        {
            this.GiftBox = AEData.GiftBoxData.CreateEffect<GiftBox>();
            RegisterEffect(GiftBox);
        }
    }


    [Serializable]
    public class GiftBox : StateEffect<GiftBox, GiftBoxData>
    {
        public override State<GiftBoxData> GetState(TechnoStatusScript statusScript)
        {
            return statusScript.GiftBoxState;
        }
        
        public override State<GiftBoxData> GetState(BulletStatusScript statusScript)
        {
            return null;
        }

    }
}
