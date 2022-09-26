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
    public class DestroySelfState : State<DestroySelfData>
    {
        public bool GoDie;

        private int timeLeft;
        private TimerStruct CountdownTimer;

        public void DestroyNow(bool peaceful)
        {
            DestroySelfData data = new DestroySelfData();
            data.Peaceful = peaceful;
            Enable(data);
        }

        public override void OnEnable()
        {
            Reset();
        }

        public void Reset()
        {
            this.GoDie = false;
            this.timeLeft = Data.Delay;
            if (timeLeft > 0)
            {
                CountdownTimer.Start(timeLeft);
            }
        }

        public bool AmIDead()
        {
            return IsActive() && !GoDie && Timeup();
        }

        private bool Timeup()
        {
            GoDie = timeLeft <= 0 || CountdownTimer.Expired();
            return GoDie;
        }
    }

}
