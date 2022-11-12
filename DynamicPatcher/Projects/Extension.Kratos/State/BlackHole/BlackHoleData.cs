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

    public partial class AttachEffectData
    {
        public BlackHoleData BlackHoleData;

        private void ReadBlackHoleData(IConfigReader reader)
        {
            BlackHoleData data = new BlackHoleData();
            data.Read(reader);
            if (data.Enable)
            {
                this.BlackHoleData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class BlackHole
    {
        public float Range;
        public int Rate;

        public BlackHole()
        {
            this.Range = 0;
            this.Rate = 0;
        }

        public BlackHole Clone()
        {
            BlackHole data = new BlackHole();
            data.Range = this.Range;
            data.Rate = this.Rate;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Range = reader.Get(title + "Range", this.Range);
            this.Rate = reader.Get(title + "Rate", this.Rate);
        }
    }

    [Serializable]
    public class BlackHoleData : FilterEffectData, IStateData
    {
        public const string TITLE = "BlackHole.";

        public BlackHole Data;
        public BlackHole EliteData;

        public CoordStruct Offset;
        public bool IsOnTurret;
        public int TriggeredTimes;

        public double Weight;
        public bool CaptureFromSameWeight;
        public int CaptureSpeed;
        public bool CaptureIgnoreWeight;
        public bool AllowEscape;
        public bool AllowCrawl;
        public bool AllowRotateUnit;
        public bool AllowPassBuilding;

        public int Damage;
        public int DamageDelay;
        public string DamageWH;
        public bool AllowFallingDestroy;
        public int FallingDestroyHeight;
        public bool AllowDamageTechno;
        public bool AllowDamageBullet;

        public bool ClearTarget;
        public bool ChangeTarget;
        public bool OutOfControl;

        public bool AffectBlackHole;

        public BlackHoleData()
        {
            this.Data = null;
            this.EliteData = null;

            this.Offset = default;
            this.IsOnTurret = true;
            this.TriggeredTimes = -1;

            this.Weight = -1;
            this.CaptureFromSameWeight = true;
            this.CaptureSpeed = (int)(12 * 2.55); // 不四舍五入
            this.CaptureIgnoreWeight = false;
            this.AllowEscape = false;
            this.AllowCrawl = true;
            this.AllowRotateUnit = true;
            this.AllowPassBuilding = false;

            this.Damage = 0;
            this.DamageDelay = 0;
            this.DamageWH = null;
            this.AllowFallingDestroy = false;
            this.FallingDestroyHeight = 2 * Game.LevelHeight;
            this.AllowDamageTechno = true;
            this.AllowDamageBullet = false;

            this.ClearTarget = false;
            this.ChangeTarget = false;
            this.OutOfControl = false;

            this.AffectBlackHole = true;

            this.AffectTechno = false;

            this.AffectsOwner = false;
            this.AffectsAllies = false;
            this.AffectsEnemies = true;
            this.AffectsCivilian = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            BlackHole data = new BlackHole();
            data.Read(reader, TITLE);
            if (data.Range > 0)
            {
                this.Data = data;
            }

            BlackHole elite = null != this.Data ? Data.Clone() : new BlackHole();
            elite.Read(reader, TITLE + "Elite");
            if (elite.Range > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

            this.Offset = reader.Get(TITLE + "Offset", this.Offset);
            this.IsOnTurret = reader.Get(TITLE + "IsOnTurret", this.IsOnTurret);
            this.TriggeredTimes = reader.Get(TITLE + "Count", this.TriggeredTimes); // 兼容
            this.TriggeredTimes = reader.Get(TITLE + "TriggeredTimes", this.TriggeredTimes);

            this.Weight = reader.Get(TITLE + "Weight", this.Weight);
            this.CaptureFromSameWeight = reader.Get(TITLE + "CaptureFromSameWeight", this.CaptureFromSameWeight);
            int speed = reader.Get(TITLE + "CaptureSpeed", 0);
            if (speed != 0)
            {
                this.CaptureSpeed = (int)(speed * 2.55);
            }
            this.CaptureIgnoreWeight = reader.Get(TITLE + "CaptureIgnoreWeight", this.CaptureIgnoreWeight);
            this.AllowEscape = reader.Get(TITLE + "AllowEscape", this.AllowEscape);
            this.AllowCrawl = reader.Get(TITLE + "AllowCrawl", this.AllowCrawl);
            this.AllowRotateUnit = reader.Get(TITLE + "AllowRotateUnit", this.AllowRotateUnit);
            this.AllowPassBuilding = reader.Get(TITLE + "AllowPassBuilding", this.AllowPassBuilding);

            this.Damage = reader.Get(TITLE + "Damage", this.Damage);
            this.DamageDelay = reader.Get(TITLE + "Damage.Delay", this.DamageDelay);
            this.DamageWH = reader.Get(TITLE + "Damage.Warhead", this.DamageWH);
            this.AllowFallingDestroy = reader.Get(TITLE + "AllowFallingDestroy", this.AllowFallingDestroy);
            this.FallingDestroyHeight = reader.Get(TITLE + "FallingDestroyHeight", this.FallingDestroyHeight);

            this.AllowDamageTechno = reader.Get(TITLE + "AllowDamageTechno", this.AllowDamageTechno);
            this.AllowDamageBullet = reader.Get(TITLE + "AllowDamageBullet", this.AllowDamageBullet);
            this.ClearTarget = reader.Get(TITLE + "ClearTarget", this.ClearTarget);
            this.ChangeTarget = reader.Get(TITLE + "ChangeTarget", this.ChangeTarget);
            this.OutOfControl = reader.Get(TITLE + "OutOfControl", this.OutOfControl);

            this.AffectBlackHole = reader.Get(TITLE + "AffectBlackHole", this.AffectBlackHole);

        }

        public int GetCaptureSpeed(double weight)
        {
            // F = mtv, v = F/mv
            if (!CaptureIgnoreWeight && weight != 0)
            {
                return (int)(this.CaptureSpeed / weight);
            }
            return this.CaptureSpeed;
        }

    }


}
