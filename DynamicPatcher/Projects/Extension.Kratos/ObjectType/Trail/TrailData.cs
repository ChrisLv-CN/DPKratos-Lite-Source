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
    public class TrailData
    {

        private IConfigWrapper<TrailType> type;
        public TrailType Type => type.Data;

        public CoordStruct FLH;
        public bool IsOnTurret;

        public bool CellLimit;
        public LandType[] OnLandTypes;
        public TileType[] OnTileTypes;

        public TrailData(IConfigWrapper<TrailType> type)
        {
            this.type = type;

            this.FLH = default;
            this.IsOnTurret = false;

            this.CellLimit = false;
            this.OnLandTypes = null;
            this.OnTileTypes = null;
        }

        public void Read(ISectionReader reader, string title)
        {
            this.FLH = reader.Get(title + "FLH", this.FLH);
            this.IsOnTurret = reader.Get(title + "IsOn", this.IsOnTurret);

            this.OnLandTypes = reader.GetList<LandType>(title + "OnLands", this.OnLandTypes);
            this.OnTileTypes = reader.GetList<TileType>(title + "OnTiles", this.OnTileTypes);
            this.CellLimit = (null != OnLandTypes && OnLandTypes.Any()) || (null != OnTileTypes && OnTileTypes.Any());
        }
    }

}