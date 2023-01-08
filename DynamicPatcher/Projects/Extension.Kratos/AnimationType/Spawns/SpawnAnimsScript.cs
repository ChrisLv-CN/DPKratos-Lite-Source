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

        private bool initFlag;
        private TimerStruct initDelayTimer;
        private TimerStruct delayTimer;
        private int spawnCount;

        public void OnUpdate_SpawnAnims()
        {
            if (spawnAnimsData.TriggerOnStart && spawnAnimsData.Count != 0)
            {
                if (!initFlag)
                {
                    ResetLoopSpawn();
                    initFlag = true;
                    int initDelay = 0;
                    if ((initDelay = spawnAnimsData.GetInitDelay()) > 0)
                    {
                        initDelayTimer.Start(initDelay);
                    }
                    else
                    {
                        initDelayTimer.Stop();
                    }
                }
                if ((spawnAnimsData.Count < 0 || spawnCount < spawnAnimsData.Count) && initDelayTimer.Expired() && delayTimer.Expired())
                {
                    ExpandAnims.PlayExpandAnims(spawnAnimsData, pAnim.Ref.Base.Base.GetCoords(), pAnim.Ref.Owner);
                    spawnCount++;
                    int delay = 0;
                    if ((delay = spawnAnimsData.GetDelay()) > 0)
                    {
                        delayTimer.Start(delay);
                    }
                }
            }
            else
            {
                ResetLoopSpawn();
            }
        }

        private void ResetLoopSpawn()
        {
            initFlag = false;
            initDelayTimer.Stop();
            delayTimer.Stop();
            spawnCount = 0;
        }

        public void OnNext_SpawnAnims(Pointer<AnimTypeClass> pNext)
        {
            ResetLoopSpawn();
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
