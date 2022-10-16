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
    public class SpawnAnimsData : ExpandAnimsData
    {
        private const string TITLE = "Spawns.";

        public bool TriggerOnDone;
        public bool TriggerOnNext;
        public bool TriggerOnLoop;

        public SpawnAnimsData() : base()
        {
            this.TriggerOnDone = true;
            this.TriggerOnNext = false;
            this.TriggerOnLoop = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.TriggerOnDone = reader.Get(TITLE + "TriggerOnDone", this.TriggerOnDone);
            this.TriggerOnNext = reader.Get(TITLE + "TriggerOnNext", this.TriggerOnNext);
            this.TriggerOnLoop = reader.Get(TITLE + "TriggerOnLoop", this.TriggerOnLoop);
        }

    }


}
