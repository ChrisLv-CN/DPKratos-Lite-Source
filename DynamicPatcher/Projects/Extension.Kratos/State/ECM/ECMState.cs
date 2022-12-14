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
        private int count;
        private int delay;
        private TimerStruct delayTimer;

        public override void OnEnable()
        {
            this.count = 0;
        }

        public bool TryGetSourceLocation(out CoordStruct location)
        {
            location = default;
            if (null != AE)
            {
                if (AE.FromWarhead)
                {
                    location = AE.WarheadLocation;
                }
                else if (!AE.pSource.IsNull)
                {
                    location = AE.pSource.Ref.Base.Base.GetCoords();
                }
            }
            return default != location;
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
