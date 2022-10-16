using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Ext
{

    public class TerrainDestroyAnim
    {

        public static void PlayDestroyAnim(Pointer<TerrainClass> pTerrain)
        {
            string section = pTerrain.Ref.Type.Ref.Base.Base.ID;
            TerrainDestroyAnimData data = Ini.GetConfig<TerrainDestroyAnimData>(Ini.RulesDependency, section).Data;
            if (null != data.Anims && data.Anims.Any())
            {
                CoordStruct location = pTerrain.Ref.Base.Base.GetCoords();
                ExpandAnims.PlayExpandAnims(data, location);
            }
        }

    }
}
