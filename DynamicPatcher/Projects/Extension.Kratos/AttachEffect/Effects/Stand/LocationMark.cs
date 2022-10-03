using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using DynamicPatcher;
using PatcherYRpp;
using PatcherYRpp.Utilities;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    [Serializable]
    public class LocationMark
    {
        public CoordStruct Location;
        public DirStruct Direction;

        public LocationMark(CoordStruct location, DirStruct direction)
        {
            this.Location = location;
            this.Direction = direction;
        }
    }

}
