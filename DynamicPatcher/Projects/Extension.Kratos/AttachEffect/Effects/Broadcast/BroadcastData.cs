using System;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Script;
using Extension.Utilities;

namespace Extension.Ext
{

    public partial class AttachEffectData
    {
        public BroadcastData BroadcastData;

        private void ReadBroadcastData(IConfigReader reader)
        {
            BroadcastData data = new BroadcastData(reader);
            if (data.Enable)
            {
                this.BroadcastData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class BroadcastEntity
    {
        public string[] Types;
        public int Rate;
        public int RangeMin;
        public int RangeMax;

        public BroadcastEntity()
        {
            this.Types = null;
            this.Rate = 15;
            this.RangeMin = 0;
            this.RangeMax = -1;
        }

        public BroadcastEntity Clone()
        {
            BroadcastEntity data = new BroadcastEntity();
            data.Types = null != this.Types ? (string[])this.Types.Clone() : null;
            data.Rate = this.Rate;
            data.RangeMin = this.RangeMin;
            data.RangeMax = this.RangeMax;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Types = reader.GetList(title + "Types", this.Types);
            this.Rate = reader.Get(title + "Rate", this.Rate);
            this.RangeMin = reader.Get(title + "RangeMin", this.RangeMin);
            this.RangeMax = reader.Get(title + "RangeMax", this.RangeMax);
        }
    }

    [Serializable]
    public class BroadcastData : FilterEffectData
    {

        public const string TITLE = "Broadcast.";

        public BroadcastEntity Data;
        public BroadcastEntity EliteData;

        public int TriggeredTimes;

        public BroadcastData()
        {
            this.Data = null;
            this.EliteData = null;

            this.TriggeredTimes = -1;

            this.AffectBullet = false;

            this.AffectsOwner = true;
            this.AffectsAllies = false;
            this.AffectsEnemies = false;
            this.AffectsCivilian = false;
        }

        public BroadcastData(IConfigReader reader) : this()
        {
            Read(reader, TITLE);
        }

        public override void Read(IConfigReader reader, string title)
        {
            base.Read(reader, title);

            BroadcastEntity data = new BroadcastEntity();
            data.Read(reader, title);
            if (null != data.Types && data.Types.Length > 0)
            {
                this.Data = data;
            }

            BroadcastEntity elite = null != this.Data ? Data.Clone() : new BroadcastEntity();
            elite.Read(reader, title + "Elite");
            if (null != elite.Types && elite.Types.Length > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

            this.TriggeredTimes = reader.Get(TITLE + "Count", this.TriggeredTimes); // 兼容
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);
        }

    }


}
