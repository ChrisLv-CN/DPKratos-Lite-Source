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

    public partial class AnimStatusScript
    {
        private SpawnAnimsData spawnAnimsData => Ini.GetConfig<SpawnAnimsData>(Ini.ArtDependency, section).Data;

        public void OnNext_SpawnAnims(Pointer<AnimTypeClass> pNext)
        {
            if (spawnAnimsData.TriggerOnNext)
            {
                ExpandAnims.PlayExpandAnims(spawnAnimsData, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

        public void OnDone_SpawnAnims()
        {
            if (spawnAnimsData.TriggerOnDone)
            {
                ExpandAnims.PlayExpandAnims(spawnAnimsData, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

        public void OnLoop_SpawnAnims()
        {
            if (spawnAnimsData.TriggerOnLoop)
            {
                ExpandAnims.PlayExpandAnims(spawnAnimsData, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

    }
}
