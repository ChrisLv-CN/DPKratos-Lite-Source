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
    public class FireSuper
    {

        public string[] Supers;
        public double[] Chances;
        public int InitDelay;
        public Point2D RandomInitDelay;
        public int Delay;
        public Point2D RandomDelay;
        public int LaunchCount;
        public bool RealLaunch;

        public int WeaponIndex;
        public bool ToTarget;


        public FireSuper()
        {
            this.Supers = null;
            this.Chances = null;
            this.InitDelay = 0;
            this.RandomInitDelay = default;
            this.Delay = 0;
            this.RandomDelay = default;
            this.LaunchCount = 1;
            this.RealLaunch = false;

            this.WeaponIndex = -1;
            this.ToTarget = true;
        }

        public FireSuper Clone()
        {
            FireSuper data = new FireSuper();
            data.Supers = null != this.Supers ? (string[])this.Supers.Clone() : null;
            data.Chances = null != this.Chances ? (double[])this.Chances.Clone() : null;
            data.InitDelay = this.InitDelay;
            data.RandomInitDelay = this.RandomInitDelay;
            data.Delay = this.Delay;
            data.RandomDelay = this.RandomDelay;
            data.LaunchCount = this.LaunchCount;
            data.RealLaunch = this.RealLaunch;
            data.WeaponIndex = this.WeaponIndex;
            data.ToTarget = this.ToTarget;
            return data;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.Supers = reader.GetList(title + "Types", this.Supers);
            this.Chances = reader.GetChanceList(title + "Chances", this.Chances);
            
            this.InitDelay = reader.Get(title + "InitDelay", this.InitDelay);
            this.RandomInitDelay = reader.Get(title + "RandomInitDelay", this.RandomInitDelay);
            this.Delay = reader.Get(title + "Delay", this.Delay);
            this.RandomDelay =  reader.Get(title + "RandomDelay", this.RandomDelay);
            this.LaunchCount = reader.Get(title + "LaunchCount", this.LaunchCount);
            this.RealLaunch = reader.Get(title + "RealLaunch", this.RealLaunch);
            this.WeaponIndex = reader.Get(title + "Weapon", this.WeaponIndex);
            this.ToTarget = reader.Get(title + "ToTarget", this.ToTarget);
        }

    }

    [Serializable]
    public class FireSuperData : EffectData, IStateData
    {
        public const string TITLE = "FireSuperWeapon.";

        public FireSuper Data;
        public FireSuper EliteData;

        public FireSuperData()
        {
            this.Data = null;
            this.EliteData = null;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            FireSuper data = new FireSuper();
            data.Read(reader, TITLE);
            if (null != data.Supers && data.Supers.Length > 0)
            {
                this.Data = data;
            }

            FireSuper elite = null != this.Data ? Data.Clone() : new FireSuper();
            elite.Read(reader, TITLE + "Elite");
            if (null != elite.Supers && elite.Supers.Length > 0)
            {
                this.EliteData = elite;
            }

            this.Enable = null != this.Data || null != this.EliteData;

        }

    }

}
