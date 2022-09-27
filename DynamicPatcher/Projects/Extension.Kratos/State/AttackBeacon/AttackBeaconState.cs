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
    public class AttackBeaconState : State<AttackBeaconData>
    {

        public static List<Mission> RecruitMissions = new List<Mission>() {
            Mission.None,
            Mission.Sleep,
            Mission.Guard,
            Mission.Area_Guard,
            Mission.Stop
        };

        public Dictionary<string, int> Types;

        private TimerStruct initialDelayTimer;
        private int delay;
        private TimerStruct delayTimer;

        public override void OnEnable()
        {
            this.Types = new Dictionary<string, int>();
            if (null != Data.Types)
            {
                for (int i = 0; i < Data.Types.Length; i++)
                {
                    string type = Data.Types[i];
                    int num = 99999;
                    if (null != Data.Nums && i < Data.Nums.Length)
                    {
                        num = Data.Nums[i];
                    }
                    Types.Add(type, num);
                }
            }
            if (Data.InitialDelay > 0)
            {
                initialDelayTimer.Start(Data.InitialDelay);
            }
        }

        public void Reload()
        {
            this.delay = Data.Rate;
            if (delay > 0)
            {
                this.delayTimer.Start(delay);
            }
        }

        public bool IsReady()
        {
            return IsActive() && initialDelayTimer.Expired() && Timeup();
        }

        private bool Timeup()
        {
            return this.delay <= 0 || delayTimer.Expired();
        }
    }

}
