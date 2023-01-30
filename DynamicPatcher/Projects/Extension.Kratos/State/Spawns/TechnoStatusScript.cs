using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using DynamicPatcher;
using PatcherYRpp;
using Extension.Ext;
using Extension.INI;
using Extension.Utilities;

namespace Extension.Script
{

    public partial class TechnoStatusScript : TechnoScriptable
    {

        private IConfigWrapper<SpawnData> _spawnData;
        public SpawnData SpawnData
        {
            get
            {
                if (null == _spawnData)
                {
                    _spawnData = Ini.GetConfig<SpawnData>(Ini.RulesDependency, section);
                }
                return _spawnData.Data;
            }
        }

        public bool TryGetSpawnType(int index, out string typeId)
        {
            typeId = null;
            if (SpawnData.MultiSpawns && index < SpawnData.Spwans.Count())
            {
                typeId = SpawnData.Spwans[index];
                return !typeId.IsNullOrEmptyOrNone();
            }
            return false;
        }

    }
}