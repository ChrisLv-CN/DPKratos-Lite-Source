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
        public BounceData BounceData;

        private void ReadBounceData(IConfigReader reader)
        {
            BounceData data = new BounceData();
            data.Read(reader);
            if (data.Enable)
            {
                this.BounceData = data;
                this.Enable = true;
            }
        }
    }

    [Serializable]
    public class BounceData : FilterEffectData, IStateData
    {
        public const string TITLE = "Bounce.";

        public double Chance; // 发生弹跳的概率

        public double Elasticity; // 弹性衰减系数
        public int Limit; // 衰减后，可以跳的距离
        public int Times; // 跳几次
        public bool ExplodeOnHit; // 命中时是否爆炸
        public string ExpireAnim; // 命中时播放动画

        public string Weapon; // 跳弹发生时使用自定义武器

        public bool OnWater; // 打水漂
        public LandType[] OnLands;
        public TileType[] OnTiles;

        public bool StopOnBuilding;
        public bool ReboundOnBuilding;
        public bool StopOnInfantry;
        public bool ReboundOnInfantry;
        public bool StopOnUnit;
        public bool ReboundOnUnit;

        public BounceData()
        {
            this.Chance = 0;

            this.Elasticity = 0.5;
            this.Limit = 128;
            this.Times = -1;
            this.ExplodeOnHit = true;
            this.ExpireAnim = null;

            this.Weapon = null;

            this.OnWater = false;
            this.OnLands = null;
            this.OnTiles = null;

            this.StopOnBuilding = true;
            this.ReboundOnBuilding = true;
            this.StopOnInfantry = false;
            this.ReboundOnInfantry = false;
            this.StopOnUnit = true;
            this.ReboundOnUnit = false;

            this.AffectTechno = false;
        }

        public override void Read(IConfigReader reader)
        {
            base.Read(reader, TITLE);

            this.Chance = reader.GetChance(TITLE + "Chance", this.Chance);
            this.Elasticity = reader.GetPercent(TITLE + "Elasticity", this.Elasticity);
            this.Enable = Chance > 0 && Elasticity > 0;
            if (Enable)
            {
                Enable = AffectBullet && AffectCannon;
            }

            this.Limit = reader.Get(TITLE + "Limit", this.Limit);
            this.Times = reader.Get(TITLE + "Times", this.Times);
            this.ExplodeOnHit = reader.Get(TITLE + "ExplodeOnHit", this.ExplodeOnHit);
            this.ExpireAnim = reader.Get(TITLE + "ExpireAnim", this.ExpireAnim);
            this.ExpireAnim = reader.Get(TITLE + "ExpireAnim", this.ExpireAnim);

            this.Weapon = reader.Get(TITLE + "Weapon", this.Weapon);

            this.OnWater = reader.Get(TITLE + "OnWater", this.OnWater);
            this.OnLands = reader.GetList<LandType>(TITLE + "OnLands", this.OnLands);
            this.OnTiles = reader.GetList<TileType>(TITLE + "OnTiles", this.OnTiles);

            this.StopOnBuilding = reader.Get(TITLE + "StopOnBuilding", this.StopOnBuilding);
            this.ReboundOnBuilding = reader.Get(TITLE + "ReboundOnBuilding", this.ReboundOnBuilding);
            this.StopOnInfantry = reader.Get(TITLE + "StopOnInfantry", this.StopOnInfantry);
            this.ReboundOnInfantry = reader.Get(TITLE + "ReboundOnInfantry", this.ReboundOnInfantry);
            this.StopOnUnit = reader.Get(TITLE + "StopOnUnit", this.StopOnUnit);
            this.ReboundOnUnit = reader.Get(TITLE + "ReboundOnUnit", this.ReboundOnUnit);
        }

        public bool IsOnLandType(Pointer<CellClass> pCell, out LandType landType)
        {
            landType = pCell.Ref.LandType;
            if (landType == LandType.Water && pCell.Ref.Flags.HasFlag(CellFlags.Bridge))
            {
                // 将水面上的桥强制判定为路面
                landType = LandType.Road;
            }
            if (null != OnLands && OnLands.Any())
            {
                return OnLands.Contains(landType);
            }
            return OnWater || landType != LandType.Water;
        }

        public bool IsOnTileType(Pointer<CellClass> pCell, out TileType tileType)
        {
            tileType = pCell.Ref.GetTileType();
            if (null != OnTiles && OnTiles.Any())
            {
                return OnTiles.Contains(tileType);
            }
            return OnWater || tileType != TileType.Water;
        }

        public bool Stop(Pointer<CellClass> pCell, out bool rebound)
        {
            bool stop = false;
            rebound = false;
            Pointer<ObjectClass> pObject = pCell.Ref.GetContent();
            do
            {
                if (!pObject.IsNull && pObject.CastToTechno(out Pointer<TechnoClass> pTarget))
                {
                    switch (pTarget.Ref.Base.Base.WhatAmI())
                    {
                        case AbstractType.Building:
                            stop = StopOnBuilding;
                            rebound = ReboundOnBuilding;
                            break;
                        case AbstractType.Infantry:
                            stop = StopOnInfantry;
                            rebound = ReboundOnInfantry;
                            break;
                        case AbstractType.Unit:
                            stop = StopOnUnit;
                            rebound = ReboundOnUnit;
                            break;
                    }
                }
            }
            while (!stop && !pObject.IsNull && !(pObject = pObject.Ref.NextObject).IsNull);
            return stop;
        }

    }


}
