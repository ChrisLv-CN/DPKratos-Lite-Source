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
    public class AircraftDockingOffsetData : INIConfig
    {
        public int[] Direction;

        public AircraftDockingOffsetData()
        {
            this.Direction = new int[12];
        }

        public override void Read(IConfigReader reader)
        {
            int poseDir = RulesClass.Global().PoseDir;
            List<int> dirs = new List<int>();
            for (int i = 0; i < 12; i++)
            {
                string key = "DockingOffset" + i + ".Dir";
                int dir = reader.Get(key, poseDir);
                if (dir < 0)
                {
                    dir = 0;
                }
                else if (dir > 7)
                {
                    dir = 7;
                }
                dirs.Add(dir);
            }
            if (dirs.Any())
            {
                this.Direction = dirs.ToArray();
            }
        }

    }


}
