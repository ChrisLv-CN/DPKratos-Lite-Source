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
    public class ExtraFireState : State<ExtraFireData>
    {

        private Dictionary<string, TimerStruct> extraFireROF = new Dictionary<string, TimerStruct>();

        public override void OnEnable()
        {
            extraFireROF.Clear();
        }

        public bool CheckROF(string weaponId, int rof)
        {
            bool canFire = false;
            if (extraFireROF.TryGetValue(weaponId, out TimerStruct rofTimer))
            {
                if (rofTimer.Expired())
                {
                    canFire = true;
                    rofTimer.Start(rof);
                    extraFireROF[weaponId] = rofTimer;
                }
            }
            else
            {
                canFire = true;
                extraFireROF.Add(weaponId, new TimerStruct(rof));
            }
            return canFire;
        }

    }

}
