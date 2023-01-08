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

        public bool TriggerOnStart;
        public int Count;

        public int InitDelay;
        public bool UseRandomInitDelay;
        public Point2D RandomInitDelay;

        public int Delay;
        public bool UseRandomDelay;
        public Point2D RandomDelay;

        public SpawnAnimsData() : base()
        {
            this.TriggerOnDone = true;
            this.TriggerOnNext = false;
            this.TriggerOnLoop = false;

            this.TriggerOnStart = false;
            this.Count = 1;

            this.InitDelay = 0;
            this.UseRandomInitDelay = false;
            this.RandomInitDelay = default;

            this.Delay = 0;
            this.UseRandomDelay = false;
            this.RandomDelay = default;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.TriggerOnDone = reader.Get(TITLE + "TriggerOnDone", this.TriggerOnDone);
            this.TriggerOnNext = reader.Get(TITLE + "TriggerOnNext", this.TriggerOnNext);
            this.TriggerOnLoop = reader.Get(TITLE + "TriggerOnLoop", this.TriggerOnLoop);

            this.TriggerOnStart = reader.Get(TITLE + "TriggerOnStart", this.TriggerOnStart);
            this.Count = reader.Get(TITLE + "Count", this.Count);

            this.InitDelay = reader.Get(TITLE + "InitDelay", this.InitDelay);
            this.RandomInitDelay = reader.Get(TITLE + "RandomInitDelay", this.RandomInitDelay);
            this.UseRandomInitDelay = default != RandomInitDelay;

            this.Delay = reader.Get(TITLE + "Delay", this.Delay);
            this.RandomDelay = reader.Get(TITLE + "RandomDelay", this.RandomDelay);
            this.UseRandomDelay = default != RandomDelay;
        }

        public int GetInitDelay()
        {
            if (UseRandomInitDelay)
            {
                return RandomInitDelay.GetRandomValue(0);
            }
            return InitDelay;
        }

        public int GetDelay()
        {
            if (UseRandomDelay)
            {
                return RandomDelay.GetRandomValue(0);
            }
            return Delay;
        }

    }


}
