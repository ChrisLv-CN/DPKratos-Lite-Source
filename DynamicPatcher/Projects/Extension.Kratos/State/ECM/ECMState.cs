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
    public class ECMState : State<ECMData>
    {
        public bool HasSourceLocation;
        public CoordStruct SourceLocation;

        private int count;
        private int delay;
        private TimerStruct delayTimer;

        public override void OnEnable()
        {
            this.count = 0;
            this.SourceLocation = default;
            if (null != AE)
            {
                if (AE.FromWarhead)
                {
                    this.SourceLocation = AE.WarheadLocation;
                }
                else if (!AE.pSource.IsNull)
                {
                    this.SourceLocation = AE.pSource.Ref.Base.Base.GetCoords();
                }
            }
            this.HasSourceLocation = default != SourceLocation;
        }

        public void Reload()
        {
            this.delay = Data.Rate;
            if (delay > 0)
            {
                this.delayTimer.Start(delay);
            }
            count++;
            if (IsDone())
            {
                Disable();
            }
        }

        public bool IsReady()
        {
            return IsActive() && !IsDone() && Timeup();
        }

        private bool Timeup()
        {
            return this.delay <= 0 || delayTimer.Expired();
        }

        private bool IsDone()
        {
            return Data.TriggeredTimes > 0 && count >= Data.TriggeredTimes;
        }

    }

}
