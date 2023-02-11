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
    public class SpawnData : INIConfig
    {
        public bool MultiSpawns;
        public string[] Spwans;

        public int SpawnDelay;



        public bool NoShadowSpawnAlt = false;

        public bool SpawnFireOnce = false;

        public SpawnData()
        {
            this.Spwans = null;

            this.SpawnDelay = -1;

            this.NoShadowSpawnAlt = false;
            this.SpawnFireOnce = false;
        }

        public override void Read(IConfigReader reader)
        {
            this.Spwans = reader.GetList("Spawns", this.Spwans);
            this.MultiSpawns = null != Spwans && Spwans.Count() > 1;

            this.SpawnDelay = reader.Get("SpawnDelay", this.SpawnDelay);

            this.NoShadowSpawnAlt = reader.Get("NoShadowSpawnAlt", this.NoShadowSpawnAlt);
            this.SpawnFireOnce = reader.Get("SpawnFireOnce", this.SpawnFireOnce);
        }

    }

}
