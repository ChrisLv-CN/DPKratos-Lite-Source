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
        public Mark Mark;

        private void InitMark()
        {
            this.Mark = AEData.MarkData.CreateEffect<Mark>();
            RegisterEffect(Mark);
        }
    }


    [Serializable]
    public class Mark : Effect<MarkData>
    {
    }
}
