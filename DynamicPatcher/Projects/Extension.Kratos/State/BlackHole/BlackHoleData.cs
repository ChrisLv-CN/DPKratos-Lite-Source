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
    public class BlackHoleData : EffectData, IStateData
    {
        public const string TITLE = "BlackHole.";

        public BlackHole Data;
        public BlackHole EliteData;

        public CoordStruct Offset;
        public int Count;

        public double Weight;
        public int CaptureSpeed;

        public int Damage;
        public int DamageDelay;
        public string DamageWH;
        public bool AllowFallingDestroy;
        public int FallingDestroyHeight;

        public string[] AffectTypes;
        public string[] NotAffectTypes;

        public bool AffectTechno;

        public bool AffectBullet;
        public bool AffectMissile;
        public bool AffectTorpedo;
        public bool AffectCannon;

        public bool AffectsOwner;
        public bool AffectsAllies;
        public bool AffectsEnemies;
        public bool AffectsCivilian;

        public BlackHoleData()
        {
            this.Data = null;
            this.EliteData = null;

            this.Offset = default;
            this.Count = -1;

            this.Weight = -1;
            this.CaptureSpeed = (int)(12 * 2.55); // 不四舍五入

            this.Damage = 0;
            this.DamageDelay = 0;
            this.DamageWH = null;
            this.AllowFallingDestroy = false;
            this.FallingDestroyHeight = 2 * Game.LevelHeight;

            this.AffectTypes = null;
            this.NotAffectTypes = null;

            this.AffectTechno = false;

            this.AffectBullet = true;
            this.AffectMissile = true;
            this.AffectTorpedo = true;
            this.AffectCannon = false;

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
            this.Count = reader.Get(TITLE + "Count", this.Count);

            this.Weight = reader.Get(TITLE + "Weight", this.Weight);
            int speed = reader.Get(TITLE + "CaptureSpeed", 0);
            if (speed != 0)
            {
                this.CaptureSpeed = (int)(speed * 2.55);
            }

            this.Damage = reader.Get(TITLE + "Damage", this.Damage);
            this.DamageDelay = reader.Get(TITLE + "Damage.Delay", this.DamageDelay);
            this.DamageWH = reader.Get(TITLE + "Damage.Warhead", this.DamageWH);
            this.AllowFallingDestroy = reader.Get(TITLE + "AllowFallingDestroy", this.AllowFallingDestroy);
            this.FallingDestroyHeight = reader.Get(TITLE + "FallingDestroyHeight", this.FallingDestroyHeight);

            this.AffectTypes = reader.GetList<string>(TITLE + "AffectTypes", this.AffectTypes);
            this.NotAffectTypes = reader.GetList<string>(TITLE + "NotAffectTypes", this.NotAffectTypes);


            this.AffectTechno = reader.Get(TITLE + "AffectTechno", this.AffectTechno);

            this.AffectMissile = reader.Get(TITLE + "AffectMissile", this.AffectMissile);
            this.AffectTorpedo = reader.Get(TITLE + "AffectTorpedo", this.AffectTorpedo);
            this.AffectCannon = reader.Get(TITLE + "AffectCannon", this.AffectCannon);

            this.AffectBullet = this.AffectMissile || this.AffectTorpedo || this.AffectCannon;

            this.AffectsOwner = reader.Get(TITLE + "AffectsOwner", this.AffectsOwner);
            this.AffectsAllies = reader.Get(TITLE + "AffectsAllies", this.AffectsAllies);
            this.AffectsEnemies = reader.Get(TITLE + "AffectsEnemies", this.AffectsEnemies);
            this.AffectsCivilian = reader.Get(TITLE + "AffectsCivilian", this.AffectsCivilian);
        }

        public bool CanAffectType(string ID)
        {
            if (null != NotAffectTypes && NotAffectTypes.Length > 0 && NotAffectTypes.Contains(ID))
            {
                return false;
            }
            return null == AffectTypes || AffectTypes.Length <= 0 || AffectTypes.Contains(ID);
        }

        public int GetCaptureSpeed(double weight)
        {
            // F = mtv, v = F/mv
            if (weight != 0)
            {
                return  (int)(this.CaptureSpeed / weight);
            }
            return this.CaptureSpeed;
        }

    }


}
