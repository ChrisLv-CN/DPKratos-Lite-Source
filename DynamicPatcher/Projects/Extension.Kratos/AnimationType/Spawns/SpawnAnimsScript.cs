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

        public void OnNext_SpawnAnims(Pointer<AnimTypeClass> pNext)
        {
            SpawnAnimsData data = Ini.GetConfig<SpawnAnimsData>(Ini.ArtDependency, section).Data;
            if (data.TriggerOnNext)
            {
                ExpandAnims.PlayExpandAnims(data, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

        public void OnDone_SpawnAnims()
        {
            SpawnAnimsData data = Ini.GetConfig<SpawnAnimsData>(Ini.ArtDependency, section).Data;
            if (data.TriggerOnDone)
            {
                ExpandAnims.PlayExpandAnims(data, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

        public void OnLoop_SpawnAnims()
        {
            SpawnAnimsData data = Ini.GetConfig<SpawnAnimsData>(Ini.ArtDependency, section).Data;
            if (data.TriggerOnLoop)
            {
                ExpandAnims.PlayExpandAnims(data, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
            }
        }

    }
}
