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
        private IConfigWrapper<SpawnAnimsData> _spawnAnimsData;
        private SpawnAnimsData spawnAnimsData
        {
            get
            {
                if (null == _spawnAnimsData)
                {
                    _spawnAnimsData = Ini.GetConfig<SpawnAnimsData>(Ini.ArtDependency, section);
                }
                return _spawnAnimsData.Data;
            }
        }

        public void OnNext_SpawnAnims(Pointer<AnimTypeClass> pNext)
        {
            // 动画next会换类型，要刷新设置
            _spawnAnimsData = null;
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
